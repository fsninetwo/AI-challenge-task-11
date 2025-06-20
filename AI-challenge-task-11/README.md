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

Ensure `BaseUrl` is set inside `appsettings.json` (defaults to the official endpoint in the template but can be changed for proxies/self-hosted gateways).

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

The core CLI workflow now lives in `TranscriptionRunner` for improved separation of concerns. `Program.cs` simply wires services and delegates execution to this runner.

After transcribing, the app now leverages a GPT model (configurable via `OpenAI:ChatModel`) to produce a concise summary, printed beneath the full transcript.

The app also extracts analytics (word count, speaking speed, frequent topics) using GPT and displays them after the summary.

## Project structure

```
WhisperTranscriberApp/
  Options/              # Configuration POCOs
  Services/
    Transcription/      # Audio transcription services and contracts
    Summarization/      # GPT summarization services and contracts
    Analytics/          # Analytics extraction services and contracts
  Runners/              # Application entry workflow(s)
  Program.cs            # Composition root
  appsettings.json      # Runtime configuration
```

Each service area lives in its own namespace (`WhisperTranscriberApp.Services.*`) to keep responsibilities clearly separated.

---
MIT License 