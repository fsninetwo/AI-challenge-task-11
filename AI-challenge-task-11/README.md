# WhisperTranscriberApp

A simple .NET 7 console application that demonstrates how to leverage dependency injection to call OpenAI's Whisper API for audio transcription.

## Prerequisites

1. **.NET 7 SDK** – Download and install from https://dotnet.microsoft.com/download/dotnet/7.0
2. **OpenAI API key** – Sign up at https://platform.openai.com/ and create an API key.

## Configuration

Set the `OPENAI_API_KEY` environment variable so the application can authenticate with the OpenAI API:

### Windows PowerShell
```powershell
$Env:OPENAI_API_KEY = "your_api_key_here"
```

### Linux / macOS / WSL
```bash
export OPENAI_API_KEY="your_api_key_here"
```

You can optionally set a different Whisper model via configuration:

```json
{
  "OpenAI": {
    "WhisperModel": "my-custom-model"
  }
}
```

Or with an environment variable:
```powershell
$Env:OpenAI__WhisperModel = "my-custom-model"
```

## Build & Run

From the repository root (where the `.sln` or project folder resides):

```bash
cd WhisperTranscriberApp

dotnet restore

dotnet run -- "../CAR0004.mp3"   # or any other audio file path
```

If no argument is provided, the application defaults to `CAR0004.mp3` in the repository root.

The console will output the transcription returned by OpenAI.

## How It Works

1. `Program.cs` sets up a generic host (`Host.CreateDefaultBuilder`) and configures dependency injection.
2. `IAudioTranscriber` defines the abstraction for transcription services.
3. `OpenAIWhisperTranscriber` implements `IAudioTranscriber` using a typed `HttpClient` to call OpenAI's `/v1/audio/transcriptions` endpoint.
4. The application reads an audio file, calls the transcriber, and prints the resulting text.

## Extending

* Swap the implementation of `IAudioTranscriber` with another provider without changing consumer code.
* Chain additional processing (e.g., GPT summarization) after the transcription.

---
MIT License 