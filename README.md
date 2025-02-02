# WhisperCLI
Whisper CLI Binaries Based on Whisper.NET

# Description
This project contains multiple binaries that can be considered plug & play. Due to using Whisper.NET as a foundation (which uses Whisper.cpp underneath), the correct installation of dependencies is no longer an issue.

# Runtimes
The following runtimes are supported:
- Windows
- Linux
- MacOS

The following platforms are supported:
- CPU (Intel, Apple, AVX, no AVX)
- GPU (CUDA Windows, CUDA Linux)

# Usage
```sh
WhisperCLI --audioFile <path> --modelFile <path> --outputFile <path> [options]
```

## Required Arguments:
- `--audioFile` : Path to audio file.
- `--modelFile` : Path to Whisper model file.
- `--outputFile` : Path for output file.

## Options:
- `--threads` : Number of threads.
- `--maxLastTextTokens` : Maximum last text tokens.
- `--offset` : Start offset (TimeSpan).
- `--duration` : Duration (TimeSpan).
- `--translate` : Translate flag.
- `--noContext` : No context flag.
- `--singleSegment` : Single segment flag.
- `--printSpecialTokens` : Print special tokens flag.
- `--printProgress` : Print progress flag.
- `--printResults` : Print results flag.
- `--printTimestamps` : Print timestamps flag.
- `--tokenTimestamps` : Token timestamps flag.
- `--tokenTimestampsThreshold` : Value for token timestamps threshold.
- `--tokenTimestampsSumThreshold` : Value for token timestamps sum threshold.
- `--maxSegmentLength` : Maximum segment length.
- `--splitOnWord` : Split on word flag.
- `--maxTokensPerSegment` : Maximum tokens per segment.
- `--audioContextSize` : Audio context size.
- `--suppressRegex` : Regex to suppress.
- `--prompt` : Prompt.
- `--language` : Language (If omitted, language detection is enabled).
- `--suppressBlank` : Suppress blank flag.
- `--temperature` : Temperature.
- `--maxInitialTs` : Max initial Ts.
- `--lengthPenalty` : Length penalty.
- `--temperatureInc` : Temperature increment.
- `--entropyThreshold` : Entropy threshold.
- `--logProbThreshold` : Log probability threshold.
- `--noSpeechThreshold` : No speech threshold.
- `--samplingStrategy` : Sampling strategy (`greedy` or `beam`).
- `--computeProbabilities` : Compute probabilities flag.
- `--openVinoEncoderPath` : OpenVino encoder path.
- `--openVinoDevice` : OpenVino device.
- `--openVinoCacheDir` : OpenVino cache directory.
- `--runtimeOrder` : Comma-separated list of runtime libraries (e.g., `Cpu,Cuda,Vulkan,CoreML,OpenVino,CpuNoAvx`).
- `--help` : Display this help message.
