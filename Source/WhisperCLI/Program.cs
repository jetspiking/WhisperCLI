using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Whisper.net;
using Whisper.net.LibraryLoader;
using Whisper.net.Internals.Native;
using Whisper.net.SamplingStrategy;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace WhisperCLI
{
    internal class Program
    {
        static async Task Main(String[] args)
        {
            if (args.Length == 0 || Array.Exists(args, arg => arg.Equals("--help", StringComparison.OrdinalIgnoreCase)))
            {
                CommandLineOptions.PrintHelp();
                return;
            }

            CommandLineOptions options = CommandLineOptions.Parse(args);
            if (options.RuntimeOrder.Provided && options.RuntimeOrder?.Value?.Count > 0)
            {
                RuntimeOptions.RuntimeLibraryOrder = options.RuntimeOrder.Value;
            }
            await ProcessAudio(options);
        }

        private static async Task ProcessAudio(CommandLineOptions options)
        {
            using WhisperFactory whisperFactory = WhisperFactory.FromPath(options.ModelFile);
            WhisperProcessorBuilder builder = whisperFactory.CreateBuilder();

            if (options.Threads.Provided)
                builder.WithThreads(options.Threads.Value);
            if (options.MaxLastTextTokens.Provided)
                builder.WithMaxLastTextTokens(options.MaxLastTextTokens.Value);
            if (options.Offset.Provided)
                builder.WithOffset(options.Offset.Value);
            if (options.Duration.Provided)
                builder.WithDuration(options.Duration.Value);
            if (options.Translate.Provided && options.Translate.Value)
                builder.WithTranslate();
            if (options.NoContext.Provided && options.NoContext.Value)
                builder.WithNoContext();
            if (options.SingleSegment.Provided && options.SingleSegment.Value)
                builder.WithSingleSegment();
            if (options.PrintSpecialTokens.Provided && options.PrintSpecialTokens.Value)
                builder.WithPrintSpecialTokens();
            if (options.PrintProgress.Provided && options.PrintProgress.Value)
                builder.WithPrintProgress();
            if (options.PrintResults.Provided && options.PrintResults.Value)
                builder.WithPrintResults();
            if (options.PrintTimestamps.Provided && options.PrintTimestamps.Value)
                builder.WithPrintTimestamps(true);
            if (options.TokenTimestamps.Provided && options.TokenTimestamps.Value)
                builder.WithTokenTimestamps();
            if (options.TokenTimestampsThreshold.Provided)
                builder.WithTokenTimestampsThreshold(options.TokenTimestampsThreshold.Value);
            if (options.TokenTimestampsSumThreshold.Provided)
                builder.WithTokenTimestampsSumThreshold(options.TokenTimestampsSumThreshold.Value);
            if (options.MaxSegmentLength.Provided)
                builder.WithMaxSegmentLength(options.MaxSegmentLength.Value);
            if (options.SplitOnWord.Provided && options.SplitOnWord.Value)
                builder.SplitOnWord();
            if (options.MaxTokensPerSegment.Provided)
                builder.WithMaxTokensPerSegment(options.MaxTokensPerSegment.Value);
            if (options.AudioContextSize.Provided)
                builder.WithAudioContextSize(options.AudioContextSize.Value);
            if (options.SuppressRegex.Provided && !String.IsNullOrEmpty(options.SuppressRegex.Value))
                builder.WithSuppressRegex(options.SuppressRegex.Value);
            if (options.Prompt.Provided && !String.IsNullOrEmpty(options.Prompt.Value))
                builder.WithPrompt(options.Prompt.Value);
            if (options.Language.Provided && !String.IsNullOrEmpty(options.Language.Value))
                builder.WithLanguage(options.Language.Value);
            else
                builder.WithLanguageDetection();
            if (options.SuppressBlank.Provided && options.SuppressBlank.Value == false)
                builder.WithoutSuppressBlank();
            if (options.Temperature.Provided)
                builder.WithTemperature(options.Temperature.Value);
            if (options.MaxInitialTs.Provided)
                builder.WithMaxInitialTs(options.MaxInitialTs.Value);
            if (options.LengthPenalty.Provided)
                builder.WithLengthPenalty(options.LengthPenalty.Value);
            if (options.TemperatureInc.Provided)
                builder.WithTemperatureInc(options.TemperatureInc.Value);
            if (options.EntropyThreshold.Provided)
                builder.WithEntropyThreshold(options.EntropyThreshold.Value);
            if (options.LogProbThreshold.Provided)
                builder.WithLogProbThreshold(options.LogProbThreshold.Value);
            if (options.NoSpeechThreshold.Provided)
                builder.WithNoSpeechThreshold(options.NoSpeechThreshold.Value);
            if (options.SamplingStrategy.Provided && !String.IsNullOrEmpty(options.SamplingStrategy.Value))
            {
                if (options.SamplingStrategy.Value.Equals("greedy", StringComparison.OrdinalIgnoreCase))
                    builder.WithGreedySamplingStrategy();
                else if (options.SamplingStrategy.Value.Equals("beam", StringComparison.OrdinalIgnoreCase))
                    builder.WithBeamSearchSamplingStrategy();
            }
            if (options.ComputeProbabilities.Provided && options.ComputeProbabilities.Value)
                builder.WithProbabilities();
            if ((options.OpenVinoEncoderPath.Provided && !String.IsNullOrEmpty(options.OpenVinoEncoderPath.Value)) ||
                (options.OpenVinoDevice.Provided && !String.IsNullOrEmpty(options.OpenVinoDevice.Value)) ||
                (options.OpenVinoCacheDir.Provided && !String.IsNullOrEmpty(options.OpenVinoCacheDir.Value)))
            {
                builder.WithOpenVinoEncoder(
                    options.OpenVinoEncoderPath.Provided ? options.OpenVinoEncoderPath.Value : null,
                    options.OpenVinoDevice.Provided ? options.OpenVinoDevice.Value : null,
                    options.OpenVinoCacheDir.Provided ? options.OpenVinoCacheDir.Value : null);
            }

            using WhisperProcessor processor = builder.Build();
            using FileStream fileStream = File.OpenRead(options.AudioFile);

            List<SegmentData> segmentDataList = new();
            await foreach (SegmentData segment in processor.ProcessAsync(fileStream))
            {
                Console.WriteLine(JsonConvert.SerializeObject(segment));
                segmentDataList.Add(segment);
            }

            File.WriteAllText(options.OutputFile, JsonConvert.SerializeObject(segmentDataList));
        }
    }

    internal class CommandLineOptions
    {
        public String AudioFile { get; set; } = String.Empty;
        public String ModelFile { get; set; } = String.Empty;
        public String OutputFile { get; set; } = String.Empty;

        public OptionalArgument<List<RuntimeLibrary>> RuntimeOrder { get; set; } = new OptionalArgument<List<RuntimeLibrary>>();
        public OptionalArgument<Int32> Threads { get; set; } = new OptionalArgument<Int32>();
        public OptionalArgument<Int32> MaxLastTextTokens { get; set; } = new OptionalArgument<Int32>();
        public OptionalArgument<TimeSpan> Offset { get; set; } = new OptionalArgument<TimeSpan>();
        public OptionalArgument<TimeSpan> Duration { get; set; } = new OptionalArgument<TimeSpan>();
        public OptionalArgument<Boolean> Translate { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> NoContext { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> SingleSegment { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> PrintSpecialTokens { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> PrintProgress { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> PrintResults { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> PrintTimestamps { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Boolean> TokenTimestamps { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Single> TokenTimestampsThreshold { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> TokenTimestampsSumThreshold { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Int32> MaxSegmentLength { get; set; } = new OptionalArgument<Int32>();
        public OptionalArgument<Boolean> SplitOnWord { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Int32> MaxTokensPerSegment { get; set; } = new OptionalArgument<Int32>();
        public OptionalArgument<Int32> AudioContextSize { get; set; } = new OptionalArgument<Int32>();
        public OptionalArgument<String> SuppressRegex { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<String> Prompt { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<String> Language { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<Boolean> SuppressBlank { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<Single> Temperature { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> MaxInitialTs { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> LengthPenalty { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> TemperatureInc { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> EntropyThreshold { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> LogProbThreshold { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<Single> NoSpeechThreshold { get; set; } = new OptionalArgument<Single>();
        public OptionalArgument<String> SamplingStrategy { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<Boolean> ComputeProbabilities { get; set; } = new OptionalArgument<Boolean>();
        public OptionalArgument<String> OpenVinoEncoderPath { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<String> OpenVinoDevice { get; set; } = new OptionalArgument<String>();
        public OptionalArgument<String> OpenVinoCacheDir { get; set; } = new OptionalArgument<String>();

        public static CommandLineOptions Parse(String[] args)
        {
            CommandLineOptions options = new CommandLineOptions();
            List<RuntimeLibrary> runtimeList = new List<RuntimeLibrary>();

            for (Int32 i = 0; i < args.Length; i++)
            {
                String arg = args[i];

                if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    Environment.Exit(0);
                }
                else if (arg.Equals("--audioFile", StringComparison.OrdinalIgnoreCase))
                {
                    options.AudioFile = args[++i];
                }
                else if (arg.Equals("--modelFile", StringComparison.OrdinalIgnoreCase))
                {
                    options.ModelFile = args[++i];
                }
                else if (arg.Equals("--outputFile", StringComparison.OrdinalIgnoreCase))
                {
                    options.OutputFile = args[++i];
                }
                else if (arg.Equals("--threads", StringComparison.OrdinalIgnoreCase))
                {
                    options.Threads = new OptionalArgument<Int32>(Int32.Parse(args[++i]));
                }
                else if (arg.Equals("--maxLastTextTokens", StringComparison.OrdinalIgnoreCase))
                {
                    options.MaxLastTextTokens = new OptionalArgument<Int32>(Int32.Parse(args[++i]));
                }
                else if (arg.Equals("--offset", StringComparison.OrdinalIgnoreCase))
                {
                    options.Offset = new OptionalArgument<TimeSpan>(TimeSpan.Parse(args[++i]));
                }
                else if (arg.Equals("--duration", StringComparison.OrdinalIgnoreCase))
                {
                    options.Duration = new OptionalArgument<TimeSpan>(TimeSpan.Parse(args[++i]));
                }
                else if (arg.Equals("--translate", StringComparison.OrdinalIgnoreCase))
                {
                    options.Translate = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--noContext", StringComparison.OrdinalIgnoreCase))
                {
                    options.NoContext = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--singleSegment", StringComparison.OrdinalIgnoreCase))
                {
                    options.SingleSegment = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--printSpecialTokens", StringComparison.OrdinalIgnoreCase))
                {
                    options.PrintSpecialTokens = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--printProgress", StringComparison.OrdinalIgnoreCase))
                {
                    options.PrintProgress = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--printResults", StringComparison.OrdinalIgnoreCase))
                {
                    options.PrintResults = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--printTimestamps", StringComparison.OrdinalIgnoreCase))
                {
                    options.PrintTimestamps = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--tokenTimestamps", StringComparison.OrdinalIgnoreCase))
                {
                    options.TokenTimestamps = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--tokenTimestampsThreshold", StringComparison.OrdinalIgnoreCase))
                {
                    options.TokenTimestampsThreshold = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--tokenTimestampsSumThreshold", StringComparison.OrdinalIgnoreCase))
                {
                    options.TokenTimestampsSumThreshold = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--maxSegmentLength", StringComparison.OrdinalIgnoreCase))
                {
                    options.MaxSegmentLength = new OptionalArgument<Int32>(Int32.Parse(args[++i]));
                }
                else if (arg.Equals("--splitOnWord", StringComparison.OrdinalIgnoreCase))
                {
                    options.SplitOnWord = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--maxTokensPerSegment", StringComparison.OrdinalIgnoreCase))
                {
                    options.MaxTokensPerSegment = new OptionalArgument<Int32>(Int32.Parse(args[++i]));
                }
                else if (arg.Equals("--audioContextSize", StringComparison.OrdinalIgnoreCase))
                {
                    options.AudioContextSize = new OptionalArgument<Int32>(Int32.Parse(args[++i]));
                }
                else if (arg.Equals("--suppressRegex", StringComparison.OrdinalIgnoreCase))
                {
                    options.SuppressRegex = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--prompt", StringComparison.OrdinalIgnoreCase))
                {
                    options.Prompt = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--language", StringComparison.OrdinalIgnoreCase))
                {
                    options.Language = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--suppressBlank", StringComparison.OrdinalIgnoreCase))
                {
                    options.SuppressBlank = new OptionalArgument<Boolean>(Boolean.Parse(args[++i]));
                }
                else if (arg.Equals("--temperature", StringComparison.OrdinalIgnoreCase))
                {
                    options.Temperature = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--maxInitialTs", StringComparison.OrdinalIgnoreCase))
                {
                    options.MaxInitialTs = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--lengthPenalty", StringComparison.OrdinalIgnoreCase))
                {
                    options.LengthPenalty = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--temperatureInc", StringComparison.OrdinalIgnoreCase))
                {
                    options.TemperatureInc = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--entropyThreshold", StringComparison.OrdinalIgnoreCase))
                {
                    options.EntropyThreshold = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--logProbThreshold", StringComparison.OrdinalIgnoreCase))
                {
                    options.LogProbThreshold = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--noSpeechThreshold", StringComparison.OrdinalIgnoreCase))
                {
                    options.NoSpeechThreshold = new OptionalArgument<Single>(Single.Parse(args[++i]));
                }
                else if (arg.Equals("--samplingStrategy", StringComparison.OrdinalIgnoreCase))
                {
                    options.SamplingStrategy = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--computeProbabilities", StringComparison.OrdinalIgnoreCase))
                {
                    options.ComputeProbabilities = new OptionalArgument<Boolean>(true);
                }
                else if (arg.Equals("--openVinoEncoderPath", StringComparison.OrdinalIgnoreCase))
                {
                    options.OpenVinoEncoderPath = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--openVinoDevice", StringComparison.OrdinalIgnoreCase))
                {
                    options.OpenVinoDevice = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--openVinoCacheDir", StringComparison.OrdinalIgnoreCase))
                {
                    options.OpenVinoCacheDir = new OptionalArgument<String>(args[++i]);
                }
                else if (arg.Equals("--runtimeOrder", StringComparison.OrdinalIgnoreCase))
                {
                    String[] libraries = args[++i].Split(',');
                    foreach (String lib in libraries)
                    {
                        if (Enum.TryParse<RuntimeLibrary>(lib, true, out RuntimeLibrary parsedLib))
                            runtimeList.Add(parsedLib);
                    }
                    options.RuntimeOrder = new OptionalArgument<List<RuntimeLibrary>>(runtimeList);
                }
                else
                {
                    Console.Error.WriteLine($"Unrecognized argument: {arg}. Use --help for a list of commands.");
                    Environment.Exit(1);
                }
            }

            return options;
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: WhisperCLI --audioFile <path> --modelFile <path> --outputFile <path> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --audioFile                    Path to audio file.");
            Console.WriteLine("  --modelFile                    Path to Whisper model file.");
            Console.WriteLine("  --outputFile                   Path for output file.");
            Console.WriteLine("  --threads                      Number of threads.");
            Console.WriteLine("  --maxLastTextTokens            Maximum last text tokens.");
            Console.WriteLine("  --offset                       Start offset (TimeSpan).");
            Console.WriteLine("  --duration                     Duration (TimeSpan).");
            Console.WriteLine("  --translate                    Translate flag.");
            Console.WriteLine("  --noContext                    No context flag.");
            Console.WriteLine("  --singleSegment                Single segment flag.");
            Console.WriteLine("  --printSpecialTokens           Print special tokens flag.");
            Console.WriteLine("  --printProgress                Print progress flag.");
            Console.WriteLine("  --printResults                 Print results flag.");
            Console.WriteLine("  --printTimestamps              Print timestamps flag.");
            Console.WriteLine("  --tokenTimestamps              Token timestamps flag.");
            Console.WriteLine("  --tokenTimestampsThreshold     Value for token timestamps threshold.");
            Console.WriteLine("  --tokenTimestampsSumThreshold  Value for token timestamps sum threshold.");
            Console.WriteLine("  --maxSegmentLength             Maximum segment length.");
            Console.WriteLine("  --splitOnWord                  Split on word flag.");
            Console.WriteLine("  --maxTokensPerSegment          Maximum tokens per segment.");
            Console.WriteLine("  --audioContextSize             Audio context size.");
            Console.WriteLine("  --suppressRegex                Regex to suppress.");
            Console.WriteLine("  --prompt                       Prompt.");
            Console.WriteLine("  --language                     Language. (If omitted, language detection is enabled)");
            Console.WriteLine("  --suppressBlank                Suppress blank flag.");
            Console.WriteLine("  --temperature                  Temperature.");
            Console.WriteLine("  --maxInitialTs                 Max initial Ts.");
            Console.WriteLine("  --lengthPenalty                Length penalty.");
            Console.WriteLine("  --temperatureInc               Temperature increment.");
            Console.WriteLine("  --entropyThreshold             Entropy threshold.");
            Console.WriteLine("  --logProbThreshold             Log probability threshold.");
            Console.WriteLine("  --noSpeechThreshold            No speech threshold.");
            Console.WriteLine("  --samplingStrategy             Sampling strategy (\"greedy\" or \"beam\").");
            Console.WriteLine("  --computeProbabilities         Compute probabilities flag.");
            Console.WriteLine("  --openVinoEncoderPath          OpenVino encoder path.");
            Console.WriteLine("  --openVinoDevice               OpenVino device.");
            Console.WriteLine("  --openVinoCacheDir             OpenVino cache directory.");
            Console.WriteLine("  --runtimeOrder                 Comma separated list of runtime libraries (e.g. Cpu,Cuda,Vulkan,CoreML,OpenVino,CpuNoAvx).");
            Console.WriteLine("  --help                         Display this help message.");
            Console.WriteLine("=====================================================================");
            Console.WriteLine("This software is available for free & published open source under the MIT license:");
            Console.WriteLine("https://github.com/jetspiking/WhisperCLI");
            Console.WriteLine("=====================================================================");
            Console.WriteLine();
        }
    }

    internal class OptionalArgument<T>
    {
        public Boolean Provided { get; private set; }
        public T? Value { get; private set; }

        public OptionalArgument()
        {
            Provided = false;
        }

        public OptionalArgument(T value)
        {
            Value = value;
            Provided = true;
        }
    }
}
