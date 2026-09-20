using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal sealed record PublishedTerminalResult(int ExitCode, string Transcript);

internal static class PublishedWindowsTerminal
{
    private const uint CreateUnicodeEnvironment = 0x00000400;
    private const uint ExtendedStartupInfoPresent = 0x00080000;
    private const uint Infinite = 0xFFFFFFFF;
    private const uint StartfUseShowWindow = 0x00000001;
    private const uint StartfUseStdHandles = 0x00000100;
    private const ushort SwHide = 0;
    private const uint WaitObject = 0x00000000;
    private const uint WaitTimeout = 0x00000102;
    private const uint WaitFailed = 0xFFFFFFFF;
    private const uint ThreadTerminate = 0x00000001;
    private const int ErrorBrokenPipe = 109;
    private const int ErrorHandleEof = 38;
    private const int ErrorInsufficientBuffer = 122;
    private const int ErrorAccessDenied = 5;
    private const int ErrorOperationAborted = 995;
    private const int ErrorNotFound = 1168;
    private const long ProcThreadAttributePseudoConsole = 0x00020016;
    private const int OutputBufferSize = 8 * 1024;
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(30);
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly UnicodeEncoding Utf16 = new(
        bigEndian: false,
        byteOrderMark: false,
        throwOnInvalidBytes: true);

    internal static async Task<PublishedTerminalResult> RunAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        string input)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(environmentVariables);
        ArgumentNullException.ThrowIfNull(input);

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "PublishedWindowsTerminal requires Windows ConPTY support.");
        }

        foreach (var argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument);
        }

        foreach (var variable in environmentVariables)
        {
            ArgumentNullException.ThrowIfNull(variable.Key);
            ArgumentNullException.ThrowIfNull(variable.Value);
        }

        return await RunWindowsAsync(
            target,
            workingDirectory,
            arguments,
            environmentVariables,
            input,
            TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    private static async Task<PublishedTerminalResult> RunWindowsAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        string input,
        CancellationToken cancellationToken)
    {
        nint ptyInputRead = nint.Zero;
        nint ptyInputWrite = nint.Zero;
        nint ptyOutputRead = nint.Zero;
        nint ptyOutputWrite = nint.Zero;
        nint pseudoConsole = nint.Zero;
        nint attributeList = nint.Zero;
        nint applicationName = nint.Zero;
        nint commandLine = nint.Zero;
        nint environmentBlock = nint.Zero;
        nint currentDirectory = nint.Zero;
        nint processHandle = nint.Zero;
        nint threadHandle = nint.Zero;
        Task? processWait = null;
        Task<string>? outputReader = null;
        TerminalInputWriter? inputWriter = null;
        var processCreated = false;
        var attributeListInitialized = false;
        Exception? primaryFailure = null;
        Exception? secondaryFailure = null;
        string? transcript = null;
        int? exitCode = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pipeAttributes = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>(),
                InheritHandle = false,
            };
            CreatePipe(ref ptyInputRead, ref ptyInputWrite, ref pipeAttributes);
            CreatePipe(ref ptyOutputRead, ref ptyOutputWrite, ref pipeAttributes);

            var createPseudoConsoleResult = CreatePseudoConsole(
                new Coord(120, 30),
                ptyInputRead,
                ptyOutputWrite,
                0,
                out pseudoConsole);
            if (createPseudoConsoleResult < 0)
            {
                Marshal.ThrowExceptionForHR(createPseudoConsoleResult);
            }

            nuint attributeListSize = 0;
            if (InitializeProcThreadAttributeList(
                    nint.Zero,
                    1,
                    0,
                    ref attributeListSize))
            {
                throw new InvalidOperationException(
                    "InitializeProcThreadAttributeList unexpectedly succeeded without storage.");
            }

            var attributeListError = Marshal.GetLastWin32Error();
            if (attributeListError != ErrorInsufficientBuffer || attributeListSize == 0)
            {
                throw new Win32Exception(
                    attributeListError,
                    "InitializeProcThreadAttributeList did not report its required buffer size.");
            }

            attributeList = Marshal.AllocHGlobal(checked((nint)attributeListSize));
            if (!InitializeProcThreadAttributeList(
                    attributeList,
                    1,
                    0,
                    ref attributeListSize))
            {
                ThrowLastWin32Error("InitializeProcThreadAttributeList");
            }

            attributeListInitialized = true;

            if (!UpdateProcThreadAttribute(
                    attributeList,
                    0,
                    (nuint)ProcThreadAttributePseudoConsole,
                    pseudoConsole,
                    (nuint)IntPtr.Size,
                    nint.Zero,
                    nint.Zero))
            {
                ThrowLastWin32Error("UpdateProcThreadAttribute");
            }

            applicationName = AllocateUtf16(target.ExecutablePath, appendTerminator: true);
            var commandLineText = BuildCommandLine(target.ExecutablePath, arguments);
            commandLine = AllocateUtf16(
                commandLineText,
                appendTerminator: true);
            environmentBlock = AllocateUtf16(
                BuildEnvironmentBlock(environmentVariables),
                appendTerminator: false);
            currentDirectory = AllocateUtf16(workingDirectory, appendTerminator: true);
            WriteTerminalCommand(commandLineText, workingDirectory);

            var startupInfo = new StartupInfoEx
            {
                StartupInfo = new StartupInfo
                {
                    Size = Marshal.SizeOf<StartupInfoEx>(),
                    Flags = StartfUseShowWindow | StartfUseStdHandles,
                    ShowWindow = SwHide,
                },
                AttributeList = attributeList,
            };

            var processInformation = new ProcessInformation();
            if (!CreateProcess(
                    applicationName,
                    commandLine,
                    nint.Zero,
                    nint.Zero,
                    bInheritHandles: false,
                    CreateUnicodeEnvironment | ExtendedStartupInfoPresent,
                    environmentBlock,
                    currentDirectory,
                    ref startupInfo,
                    out processInformation))
            {
                ThrowLastWin32Error("CreateProcessW");
            }

            processHandle = processInformation.ProcessHandle;
            threadHandle = processInformation.ThreadHandle;
            processCreated = true;

            CloseHandleOrThrow(ref threadHandle);
            CloseHandleOrThrow(ref ptyInputRead);
            CloseHandleOrThrow(ref ptyOutputWrite);

            outputReader = Task.Run(
                () => ReadTerminalOutput(ptyOutputRead),
                CancellationToken.None);
            processWait = WaitForProcessAsync(processHandle);
            var waitDeadline = Stopwatch.GetTimestamp();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                inputWriter = TerminalInputWriter.Create(ptyInputWrite, input);
                if (inputWriter is not null)
                {
                    inputWriter.StartAndSignalWrite(
                        GetRemainingTime(waitDeadline),
                        cancellationToken);
                }

                await WaitForProcessAndInputAsync(
                    processWait,
                    inputWriter,
                    waitDeadline,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                primaryFailure = new TimeoutException(
                    $"Executable '{target.ExecutablePath}' did not exit within {ProcessTimeout}.");
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                primaryFailure = exception;
            }
            catch (Exception exception)
            {
                primaryFailure = exception;
            }
        }
        catch (Exception exception)
        {
            RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
        }

        if (processCreated)
        {
            try
            {
                if (IsProcessRunning(processHandle))
                {
                    TerminateProcessAndConfirm(processHandle);
                }
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }

            if (processWait is not null)
            {
                try
                {
                    await processWait.ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
                }
            }
        }

        if (pseudoConsole != nint.Zero)
        {
            var pseudoConsoleToClose = pseudoConsole;
            pseudoConsole = nint.Zero;
            try
            {
                ClosePseudoConsole(pseudoConsoleToClose);
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }
        }

        // Release the owned console before joining its input writer. If native
        // I/O cancellation fails, closing the console still breaks the pipe;
        // joining first could prevent the process/console cleanup from running.
        if (inputWriter is not null)
        {
            try
            {
                inputWriter.Finish(cancel: primaryFailure is not null);
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }
        }

        if (outputReader is not null)
        {
            try
            {
                transcript = await outputReader.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }
        }

        if (primaryFailure is null && processCreated)
        {
            try
            {
                exitCode = ReadExitCode(processHandle);
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }
        }

        TryCloseHandle(ref processHandle, ref secondaryFailure);
        TryCloseHandle(ref threadHandle, ref secondaryFailure);
        TryCloseHandle(ref ptyInputRead, ref secondaryFailure);
        TryCloseHandle(ref ptyInputWrite, ref secondaryFailure);
        TryCloseHandle(ref ptyOutputRead, ref secondaryFailure);
        TryCloseHandle(ref ptyOutputWrite, ref secondaryFailure);
        TryDeleteAttributeList(
            ref attributeList,
            attributeListInitialized,
            ref secondaryFailure);
        TryFreeUnmanaged(ref applicationName, ref secondaryFailure);
        TryFreeUnmanaged(ref commandLine, ref secondaryFailure);
        TryFreeUnmanaged(ref environmentBlock, ref secondaryFailure);
        TryFreeUnmanaged(ref currentDirectory, ref secondaryFailure);

        if (transcript is not null)
        {
            try
            {
                WriteTerminalTranscript(transcript);
            }
            catch (Exception exception)
            {
                RecordFailure(ref primaryFailure, ref secondaryFailure, exception);
            }
        }

        if (primaryFailure is not null)
        {
            AttachSecondaryFailure(primaryFailure, secondaryFailure);
            ExceptionDispatchInfo.Capture(primaryFailure).Throw();
        }

        if (secondaryFailure is not null)
        {
            ExceptionDispatchInfo.Capture(secondaryFailure).Throw();
        }

        return new PublishedTerminalResult(
            exitCode ?? throw new InvalidOperationException("The child process did not produce an exit code."),
            transcript ?? throw new InvalidOperationException("The terminal reader did not produce a transcript."));
    }

    private static void CreatePipe(
        ref nint readHandle,
        ref nint writeHandle,
        ref SecurityAttributes attributes)
    {
        if (!CreatePipeNative(out readHandle, out writeHandle, ref attributes, 0))
        {
            ThrowLastWin32Error("CreatePipe");
        }
    }

    private static string BuildCommandLine(
        string executablePath,
        IReadOnlyList<string> arguments)
    {
        var commandLine = new StringBuilder();
        AppendQuotedArgument(commandLine, executablePath);
        foreach (var argument in arguments)
        {
            commandLine.Append(' ');
            AppendQuotedArgument(commandLine, argument);
        }

        return commandLine.ToString();
    }

    private static void WriteTerminalCommand(string commandLine, string workingDirectory)
        => TestContext.Current.TestOutputHelper?.WriteLine(
            $"Command: {commandLine}\nWorking directory: {workingDirectory}");

    private static void WriteTerminalTranscript(string transcript)
        => TestContext.Current.TestOutputHelper?.WriteLine(
            $"Terminal transcript (UTF-8 base64): {Convert.ToBase64String(Encoding.UTF8.GetBytes(transcript))}");

    private static void AppendQuotedArgument(StringBuilder commandLine, string argument)
    {
        if (argument.Contains('\0'))
        {
            throw new ArgumentException(
                "Windows process arguments cannot contain NUL characters.",
                nameof(argument));
        }

        commandLine.Append('"');
        var backslashes = 0;
        foreach (var character in argument)
        {
            if (character == '\\')
            {
                backslashes++;
                continue;
            }

            if (character == '"')
            {
                commandLine.Append('\\', checked(backslashes * 2 + 1));
                commandLine.Append('"');
                backslashes = 0;
                continue;
            }

            commandLine.Append('\\', backslashes);
            commandLine.Append(character);
            backslashes = 0;
        }

        commandLine.Append('\\', checked(backslashes * 2));
        commandLine.Append('"');
    }

    private static string BuildEnvironmentBlock(
        IReadOnlyDictionary<string, string> environmentVariables)
    {
        var inherited = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            if (variable.Key is not string key || variable.Value is not string value)
            {
                throw new InvalidOperationException(
                    "The process environment contained a non-string variable.");
            }

            inherited[key] = value;
        }

        foreach (var variable in environmentVariables)
        {
            ValidateEnvironmentKey(variable.Key, nameof(environmentVariables));
            ValidateEnvironmentPart(variable.Value, nameof(environmentVariables));
            inherited[variable.Key] = variable.Value;
        }

        inherited.Remove("TERM");
        inherited["TERM"] = "dumb";

        var environmentBlock = new StringBuilder();
        foreach (var variable in inherited.OrderBy(variable => variable.Key, StringComparer.OrdinalIgnoreCase))
        {
            environmentBlock.Append(variable.Key);
            environmentBlock.Append('=');
            environmentBlock.Append(variable.Value);
            environmentBlock.Append('\0');
        }

        environmentBlock.Append('\0');
        return environmentBlock.ToString();
    }

    private static void ValidateEnvironmentKey(string value, string parameterName)
    {
        ValidateEnvironmentPart(value, parameterName);
        if (value.Length == 0 || value.Contains('='))
        {
            throw new ArgumentException(
                "Windows environment variable names cannot be empty or contain '='.",
                parameterName);
        }
    }

    private static void ValidateEnvironmentPart(string value, string parameterName)
    {
        if (value.Contains('\0'))
        {
            throw new ArgumentException(
                "Windows environment names and values cannot contain NUL characters.",
                parameterName);
        }
    }

    private static nint AllocateUtf16(string value, bool appendTerminator)
    {
        var bytes = Utf16.GetBytes(value);
        var byteCount = checked(bytes.Length + (appendTerminator ? sizeof(char) : 0));
        var allocation = Marshal.AllocHGlobal(byteCount);
        try
        {
            Marshal.Copy(bytes, 0, allocation, bytes.Length);
            if (appendTerminator)
            {
                Marshal.WriteInt16(allocation, bytes.Length, 0);
            }

            return allocation;
        }
        catch
        {
            Marshal.FreeHGlobal(allocation);
            throw;
        }
    }

    private static TimeSpan GetRemainingTime(long deadlineTimestamp)
    {
        var remaining = ProcessTimeout - Stopwatch.GetElapsedTime(deadlineTimestamp);
        if (remaining <= TimeSpan.Zero)
        {
            throw new TimeoutException();
        }

        return remaining;
    }

    private static async Task WaitForProcessAndInputAsync(
        Task processWait,
        TerminalInputWriter? inputWriter,
        long deadlineTimestamp,
        CancellationToken cancellationToken)
    {
        if (inputWriter is not null)
        {
            var firstCompletion = await Task.WhenAny(processWait, inputWriter.Completion)
                .WaitAsync(GetRemainingTime(deadlineTimestamp), cancellationToken)
                .ConfigureAwait(false);
            if (firstCompletion == inputWriter.Completion)
            {
                ThrowInputWriterFailure(inputWriter);
            }
        }

        await processWait.WaitAsync(
            GetRemainingTime(deadlineTimestamp),
            cancellationToken).ConfigureAwait(false);

        if (inputWriter is not null)
        {
            await inputWriter.Completion.WaitAsync(
                GetRemainingTime(deadlineTimestamp),
                cancellationToken).ConfigureAwait(false);
            ThrowInputWriterFailure(inputWriter);
        }
    }

    private static void ThrowInputWriterFailure(TerminalInputWriter inputWriter)
    {
        var failure = inputWriter.ObserveFailure();
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private static void WriteTerminalInput(
        nint inputHandle,
        nint inputBuffer,
        int inputByteCount)
    {
        var offset = 0;
        while (offset < inputByteCount)
        {
            var remaining = checked((uint)(inputByteCount - offset));
            if (!WriteFile(
                    inputHandle,
                    IntPtr.Add(inputBuffer, offset),
                    remaining,
                    out var bytesWritten,
                    nint.Zero))
            {
                ThrowLastWin32Error("WriteFile");
            }

            if (bytesWritten == 0)
            {
                throw new IOException("WriteFile wrote no terminal input bytes.");
            }

            offset = checked(offset + (int)bytesWritten);
        }
    }

    private static Task WaitForProcessAsync(nint processHandle)
        => Task.Run(() => WaitForProcess(processHandle), CancellationToken.None);

    private static void WaitForProcess(nint processHandle)
    {
        var waitResult = WaitForSingleObject(processHandle, Infinite);
        if (waitResult == WaitObject)
        {
            return;
        }

        if (waitResult == WaitFailed)
        {
            ThrowLastWin32Error("WaitForSingleObject");
        }

        throw new InvalidOperationException(
            $"WaitForSingleObject returned unexpected result {waitResult}.");
    }

    private static bool IsProcessRunning(nint processHandle)
    {
        var waitResult = WaitForSingleObject(processHandle, 0);
        if (waitResult == WaitObject)
        {
            return false;
        }

        if (waitResult == WaitTimeout)
        {
            return true;
        }

        if (waitResult == WaitFailed)
        {
            ThrowLastWin32Error("WaitForSingleObject");
        }

        throw new InvalidOperationException(
            $"WaitForSingleObject returned unexpected result {waitResult}.");
    }

    private static void TerminateProcessAndConfirm(nint processHandle)
    {
        if (IsProcessRunning(processHandle))
        {
            if (!TerminateProcess(processHandle, 1))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ErrorAccessDenied || IsProcessRunning(processHandle))
                {
                    throw new Win32Exception(error, "TerminateProcess failed.");
                }
            }
        }

        WaitForProcess(processHandle);
    }

    private static int ReadExitCode(nint processHandle)
    {
        if (!GetExitCodeProcess(processHandle, out var exitCode))
        {
            ThrowLastWin32Error("GetExitCodeProcess");
        }

        return unchecked((int)exitCode);
    }

    private sealed class TerminalInputWriter
    {
        private const int WaitingForStart = 0;
        private const int WriteRequested = 1;
        private const int CancellationRequested = 2;

        private readonly nint _inputHandle;
        private readonly int _inputByteCount;
        private readonly ManualResetEventSlim _ready = new(initialState: false);
        private readonly ManualResetEventSlim _start = new(initialState: false);
        private readonly TaskCompletionSource<Exception?> _completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Thread _thread;
        private nint _inputBuffer;
        private nint _threadHandle;
        private int _command;
        private uint _threadId;
        private bool _threadStarted;
        private bool _failureObserved;
        private bool _finished;

        private TerminalInputWriter(nint inputHandle, byte[] input)
        {
            _inputHandle = inputHandle;
            _inputByteCount = input.Length;
            _inputBuffer = Marshal.AllocHGlobal(input.Length);
            try
            {
                Marshal.Copy(input, 0, _inputBuffer, input.Length);
            }
            catch
            {
                Marshal.FreeHGlobal(_inputBuffer);
                _inputBuffer = nint.Zero;
                throw;
            }

            try
            {
                _thread = new Thread(WriteOnThread)
                {
                    IsBackground = true,
                };
            }
            catch
            {
                Marshal.FreeHGlobal(_inputBuffer);
                _inputBuffer = nint.Zero;
                throw;
            }
        }

        internal static TerminalInputWriter? Create(nint inputHandle, string input)
        {
            var bytes = StrictUtf8.GetBytes(input);
            return bytes.Length == 0
                ? null
                : new TerminalInputWriter(inputHandle, bytes);
        }

        internal Task<Exception?> Completion => _completion.Task;

        internal Exception? ObserveFailure()
        {
            if (_failureObserved)
            {
                return null;
            }

            var failure = _completion.Task.GetAwaiter().GetResult();
            _failureObserved = true;
            return failure;
        }

        internal void StartAndSignalWrite(
            TimeSpan readinessTimeout,
            CancellationToken cancellationToken)
        {
            _thread.Start();
            _threadStarted = true;

            if (!_ready.Wait(readinessTimeout, cancellationToken))
            {
                throw new TimeoutException();
            }

            var nativeThreadId = Volatile.Read(ref _threadId);
            if (nativeThreadId == 0)
            {
                throw new InvalidOperationException(
                    "The terminal-input writer did not publish its native thread identifier.");
            }

            _threadHandle = OpenThread(
                ThreadTerminate,
                bInheritHandle: false,
                nativeThreadId);
            if (_threadHandle == nint.Zero)
            {
                ThrowLastWin32Error("OpenThread");
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (Interlocked.CompareExchange(
                    ref _command,
                    WriteRequested,
                    WaitingForStart) != WaitingForStart)
            {
                throw new OperationCanceledException();
            }

            _start.Set();
        }

        internal void Finish(bool cancel)
        {
            if (_finished)
            {
                return;
            }

            Exception? cleanupFailure = null;
            if (cancel)
            {
                try
                {
                    RequestCancellation();
                }
                catch (Exception exception)
                {
                    RecordSecondaryFailure(ref cleanupFailure, exception);
                }
            }

            if (_threadStarted)
            {
                try
                {
                    _thread.Join();
                }
                catch (Exception exception)
                {
                    RecordSecondaryFailure(ref cleanupFailure, exception);
                }
            }

            if (_completion.Task.IsCompleted)
            {
                try
                {
                    var writerFailure = ObserveFailure();
                    if (writerFailure is not null)
                    {
                        RecordSecondaryFailure(ref cleanupFailure, writerFailure);
                    }
                }
                catch (Exception exception)
                {
                    RecordSecondaryFailure(ref cleanupFailure, exception);
                }
            }

            TryCloseHandle(ref _threadHandle, ref cleanupFailure);
            TryFreeUnmanaged(ref _inputBuffer, ref cleanupFailure);
            try
            {
                _ready.Dispose();
            }
            catch (Exception exception)
            {
                RecordSecondaryFailure(ref cleanupFailure, exception);
            }

            try
            {
                _start.Dispose();
            }
            catch (Exception exception)
            {
                RecordSecondaryFailure(ref cleanupFailure, exception);
            }

            _finished = true;
            if (cleanupFailure is not null)
            {
                ExceptionDispatchInfo.Capture(cleanupFailure).Throw();
            }
        }

        private void WriteOnThread()
        {
            Exception? failure = null;
            try
            {
                Volatile.Write(ref _threadId, GetCurrentThreadId());
                _ready.Set();
                _start.Wait();
                if (Volatile.Read(ref _command) == WriteRequested)
                {
                    try
                    {
                        WriteTerminalInput(_inputHandle, _inputBuffer, _inputByteCount);
                    }
                    catch (Win32Exception exception) when (
                        Volatile.Read(ref _command) == CancellationRequested &&
                        exception.NativeErrorCode == ErrorOperationAborted)
                    {
                    }
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                _completion.TrySetResult(failure);
            }
        }

        private void RequestCancellation()
        {
            Interlocked.Exchange(ref _command, CancellationRequested);
            _start.Set();
            if (!_threadStarted || _threadHandle == nint.Zero || _completion.Task.IsCompleted)
            {
                return;
            }

            while (!_completion.Task.IsCompleted)
            {
                if (!CancelSynchronousIo(_threadHandle))
                {
                    var error = Marshal.GetLastWin32Error();
                    if (error != ErrorNotFound)
                    {
                        throw new Win32Exception(error, "CancelSynchronousIo failed.");
                    }
                }

                if (!_completion.Task.IsCompleted)
                {
                    Thread.Yield();
                }
            }
        }
    }

    private static string ReadTerminalOutput(nint outputHandle)
    {
        using var output = new MemoryStream();
        var buffer = new byte[OutputBufferSize];
        while (true)
        {
            if (!ReadFile(
                    outputHandle,
                    buffer,
                    (uint)buffer.Length,
                    out var bytesRead,
                    nint.Zero))
            {
                var error = Marshal.GetLastWin32Error();
                if (error is ErrorBrokenPipe or ErrorHandleEof)
                {
                    break;
                }

                throw new Win32Exception(error, "ReadFile failed while draining terminal output.");
            }

            if (bytesRead == 0)
            {
                break;
            }

            output.Write(buffer, 0, checked((int)bytesRead));
        }

        return StrictUtf8.GetString(output.ToArray());
    }

    private static void CloseHandleOrThrow(ref nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        var ownedHandle = handle;
        handle = nint.Zero;
        if (!CloseHandle(ownedHandle))
        {
            ThrowLastWin32Error("CloseHandle");
        }
    }

    private static void TryCloseHandle(ref nint handle, ref Exception? secondaryFailure)
    {
        try
        {
            CloseHandleOrThrow(ref handle);
        }
        catch (Exception exception)
        {
            RecordSecondaryFailure(ref secondaryFailure, exception);
        }
    }

    private static void TryDeleteAttributeList(
        ref nint attributeList,
        bool attributeListInitialized,
        ref Exception? secondaryFailure)
    {
        if (attributeList == nint.Zero)
        {
            return;
        }

        var ownedAttributeList = attributeList;
        attributeList = nint.Zero;
        if (attributeListInitialized)
        {
            try
            {
                DeleteProcThreadAttributeList(ownedAttributeList);
            }
            catch (Exception exception)
            {
                RecordSecondaryFailure(ref secondaryFailure, exception);
            }
        }

        TryFreeUnmanaged(ownedAttributeList, ref secondaryFailure);
    }

    private static void TryFreeUnmanaged(
        ref nint allocation,
        ref Exception? secondaryFailure)
    {
        if (allocation == nint.Zero)
        {
            return;
        }

        var ownedAllocation = allocation;
        allocation = nint.Zero;
        TryFreeUnmanaged(ownedAllocation, ref secondaryFailure);
    }

    private static void TryFreeUnmanaged(
        nint allocation,
        ref Exception? secondaryFailure)
    {
        try
        {
            Marshal.FreeHGlobal(allocation);
        }
        catch (Exception exception)
        {
            RecordSecondaryFailure(ref secondaryFailure, exception);
        }
    }

    private static void RecordFailure(
        ref Exception? primaryFailure,
        ref Exception? secondaryFailure,
        Exception exception)
    {
        if (primaryFailure is null)
        {
            primaryFailure = exception;
            return;
        }

        RecordSecondaryFailure(ref secondaryFailure, exception);
    }

    private static void RecordSecondaryFailure(
        ref Exception? secondaryFailure,
        Exception exception)
    {
        secondaryFailure = secondaryFailure is null
            ? exception
            : new AggregateException(
                "Published Windows terminal cleanup or drain failed.",
                secondaryFailure,
                exception);
    }

    private static void AttachSecondaryFailure(
        Exception primaryFailure,
        Exception? secondaryFailure)
    {
        if (secondaryFailure is not null)
        {
            primaryFailure.Data["PublishedWindowsTerminal.SecondaryFailure"] = secondaryFailure;
        }
    }

    private static void ThrowLastWin32Error(string operation)
        => throw new Win32Exception(Marshal.GetLastWin32Error(), $"{operation} failed.");

    [DllImport("kernel32.dll", EntryPoint = "CloseHandle", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);

    [DllImport("kernel32.dll", EntryPoint = "CancelSynchronousIo", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CancelSynchronousIo(nint threadHandle);

    [DllImport("kernel32.dll", EntryPoint = "ClosePseudoConsole", ExactSpelling = true)]
    private static extern void ClosePseudoConsole(nint pseudoConsole);

    [DllImport("kernel32.dll", EntryPoint = "CreatePipe", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreatePipeNative(
        out nint readPipe,
        out nint writePipe,
        ref SecurityAttributes pipeAttributes,
        uint size);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateProcessW",
        ExactSpelling = true,
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcess(
        nint applicationName,
        nint commandLine,
        nint processAttributes,
        nint threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
        uint creationFlags,
        nint environment,
        nint currentDirectory,
        ref StartupInfoEx startupInfo,
        out ProcessInformation processInformation);

    [DllImport("kernel32.dll", EntryPoint = "CreatePseudoConsole", ExactSpelling = true)]
    private static extern int CreatePseudoConsole(
        Coord size,
        nint inputHandle,
        nint outputHandle,
        uint flags,
        out nint pseudoConsole);

    [DllImport("kernel32.dll", EntryPoint = "DeleteProcThreadAttributeList", ExactSpelling = true)]
    private static extern void DeleteProcThreadAttributeList(nint attributeList);

    [DllImport("kernel32.dll", EntryPoint = "GetExitCodeProcess", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetExitCodeProcess(nint processHandle, out uint exitCode);

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
    private static extern uint GetCurrentThreadId();

    [DllImport("kernel32.dll", EntryPoint = "InitializeProcThreadAttributeList", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InitializeProcThreadAttributeList(
        nint attributeList,
        uint attributeCount,
        uint flags,
        ref nuint attributeListSize);

    [DllImport("kernel32.dll", EntryPoint = "ReadFile", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadFile(
        nint file,
        [Out] byte[] buffer,
        uint bytesToRead,
        out uint bytesRead,
        nint overlapped);

    [DllImport("kernel32.dll", EntryPoint = "OpenThread", ExactSpelling = true, SetLastError = true)]
    private static extern nint OpenThread(
        uint desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint threadId);

    [DllImport("kernel32.dll", EntryPoint = "TerminateProcess", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(nint processHandle, uint exitCode);

    [DllImport("kernel32.dll", EntryPoint = "UpdateProcThreadAttribute", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateProcThreadAttribute(
        nint attributeList,
        uint flags,
        nuint attribute,
        nint value,
        nuint valueSize,
        nint previousValue,
        nint returnSize);

    [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", ExactSpelling = true, SetLastError = true)]
    private static extern uint WaitForSingleObject(nint handle, uint milliseconds);

    [DllImport("kernel32.dll", EntryPoint = "WriteFile", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WriteFile(
        nint file,
        nint buffer,
        uint bytesToWrite,
        out uint bytesWritten,
        nint overlapped);

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord(short x, short y)
    {
        internal short X = x;
        internal short Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        internal nint ProcessHandle;
        internal nint ThreadHandle;
        internal uint ProcessId;
        internal uint ThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        internal int Length;
        internal nint SecurityDescriptor;

        [MarshalAs(UnmanagedType.Bool)]
        internal bool InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfo
    {
        internal int Size;
        internal nint Reserved;
        internal nint Desktop;
        internal nint Title;
        internal uint X;
        internal uint Y;
        internal uint XSize;
        internal uint YSize;
        internal uint XCountChars;
        internal uint YCountChars;
        internal uint FillAttribute;
        internal uint Flags;
        internal ushort ShowWindow;
        internal ushort Reserved2;
        internal nint Reserved2Bytes;
        internal nint StandardInput;
        internal nint StandardOutput;
        internal nint StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfoEx
    {
        internal StartupInfo StartupInfo;
        internal nint AttributeList;
    }
}
