using System.Text;

namespace MonoAlphabeticCipher_Semestralka_Andrei
{
    internal class Work
    {
        private const int    _populationSize      = 1000,
                             _bestCandidatesCount = 200,
                             _generationsCount    = 1000,
                             _logMessageFrequency = 10;
        private const string CONST_ALPHABET = "abcdefghijklmnopqrstuvwxyz";

        private string _cipherFilePath;
        private string _wordsDictionaryFilePath;
        private string _cipherString;

        private double _maxAccuracy;
        private Random _random;

        private HashSet<string>        _hashSetWords;
        private List<string>           _cipherWords;
        private Dictionary<char, char> _bestDict;

        private List<Dictionary<char, char>> _population;


        internal Work()
        {
            _cipherFilePath          = string.Empty;
            _wordsDictionaryFilePath = string.Empty;
            _cipherString            = string.Empty;

            _maxAccuracy  = 0;
            _random       = new Random((int)DateTime.Now.Ticks);

            _hashSetWords = new HashSet<string>();
            _cipherWords  = new List<string>();
            _bestDict     = new Dictionary<char, char>();

            _population = new List<Dictionary<char, char>>();
        }
        internal Work(string cipherFilePath, string wordsDictionaryFilePath) : this()
        {
            _cipherFilePath          = cipherFilePath;
            _wordsDictionaryFilePath = wordsDictionaryFilePath;
        }


        #region Additional functions

        internal void Init()
        {
            var words = File.ReadAllLines(_wordsDictionaryFilePath);
            foreach (var word in words) _hashSetWords.Add(word);

            _cipherString = File.ReadAllText(_cipherFilePath);
            _cipherWords  = _cipherString.Split(new char[2] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        }

        internal void ConsoleLog(string text)
        {
            Console.WriteLine($"[{DateTime.Now}] {text}");
        }
        internal Dictionary<char, char> ConvertTextToDictionary(string dictText)
        {
            if (string.IsNullOrEmpty(dictText)) return new Dictionary<char, char>();

            var dict = new Dictionary<char, char>();
            foreach (var pairText in dictText.Substring(dictText.IndexOf(" -> ") + 4)
                                             .Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = pairText.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                dict.Add(pair[0][0], pair[1][0]);
            }
            return dict;
        }
        private string GenerateStringForDict(Dictionary<char, char> dict)
        {
            return string.Join(",", dict.ToArray().OrderBy(d => d.Key).Select(p => p.Key + ":" + p.Value));
        }

        internal void WriteNotFoundWords(string dictText)
        {
            var decodedText = Decode(_cipherString, ConvertTextToDictionary(dictText));
            var decodedWords = decodedText.Split(new char[2] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct();
            foreach (var word in decodedWords)
            {
                if (!_hashSetWords.Contains(word)) Console.Write(word + " ");
            }
        }

        #endregion

        #region Encode and decode functions

        internal string Encode(string text)
        {
            var reversedDict = new Dictionary<char, char>();
            foreach (var entry in _bestDict) reversedDict.Add(entry.Value, entry.Key);

            return Decode(text, reversedDict);
        }
        internal string Decode()
        {
            return Decode(_cipherString, _bestDict);
        }
        internal string Decode(string dictText)
        {
            return Decode(_cipherString, ConvertTextToDictionary(dictText));
        }
        internal string Decode(string text, Dictionary<char, char> dict)
        {
            var result = new StringBuilder();
            foreach (var ch in text) result.Append(ch == ' ' || ch == '\n' ? ch : (dict.ContainsKey(ch) ? dict[ch] : '?'));
            return result.ToString();
        }

        #endregion

        #region Genetic algorithm

        private void InitializePopulation(int size)
        {
            _population = new List<Dictionary<char, char>>();
            var alphabet = CONST_ALPHABET.ToCharArray();

            for (int i = 0; i < size; i++)
            {
                var shuffledAlphabet = alphabet.OrderBy(a => _random.Next()).ToArray();
                var dict = alphabet.Zip(shuffledAlphabet, (a, b) => new { a, b }).ToDictionary(item => item.a, item => item.b);
                _population.Add(dict);
            }
        }

        private int EvaluateFitness(Dictionary<char, char> dict)
        {
            var decodedText = Decode(_cipherString, dict);
            var decodedWords = decodedText.Split(new char[2] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct();
            int score = 0;

            foreach (var word in decodedWords) if (_hashSetWords.Contains(word)) ++score;

            return score;
        }

        private List<Dictionary<char, char>> SelectBestCandidates(List<Dictionary<char, char>> population, int count)
        {
            return population.OrderByDescending(dict => EvaluateFitness(dict)).Take(count).ToList();
        }

        private Dictionary<char, char> Crossover(Dictionary<char, char> parent1, Dictionary<char, char> parent2)
        {
            var childDict  = new Dictionary<char, char>();
            var usedValues = new HashSet<char>(); // Для отслеживания уже использованных значений
            var cutoff     = _random.Next(parent1.Count);

            // Добавляем элементы из первого родителя до cutoff
            foreach (var item in parent1.Take(cutoff))
            {
                childDict[item.Key] = item.Value;
                usedValues.Add(item.Value);
            }

            // Для оставшихся ключей второго родителя добавляем значения, проверяя уникальность
            foreach (var item in parent2)
            {
                if (!childDict.ContainsKey(item.Key) && !usedValues.Contains(item.Value))
                {
                    childDict[item.Key] = item.Value;
                    usedValues.Add(item.Value);
                }
            }

            // Если после добавления всех уникальных элементов от второго родителя
            // в дочернем словаре всё ещё не хватает букв, заполняем остальные случайным образом.
            // Это может быть необходимо, если есть перекрытие в используемых значениях.
            var allLetters = CONST_ALPHABET.ToList();
            foreach (var letter in allLetters)
            {
                if (childDict.Values.Contains(letter)) continue;

                var missingKey = parent1.Keys.Concat(parent2.Keys).Except(childDict.Keys).FirstOrDefault();
                if (missingKey != default)
                {
                    childDict[missingKey] = letter;
                }
            }

            return childDict;
        }

        private void Mutate(Dictionary<char, char> dict)
        {
            var keys = dict.Keys.ToList();
            var index1 = _random.Next(keys.Count);
            var index2 = _random.Next(keys.Count);
            var temp = dict[keys[index1]];
            dict[keys[index1]] = dict[keys[index2]];
            dict[keys[index2]] = temp;
        }

        #endregion

        internal void FindSolution()
        {
            ConsoleLog("Initializing population.");
            InitializePopulation(_populationSize); // Example population size
            ConsoleLog("Let's go.");

            DateTime startTime = DateTime.Now;
            var localBestDict = new Dictionary<char, char>();
            for (int i = 1; i <= _generationsCount; i++)
            {
                bool shouldLog = i % _logMessageFrequency == _logMessageFrequency - 1;

                if (shouldLog)
                {
                    double workDonePercent = 100.0 * i / _generationsCount;
                    ConsoleLog($"Started work with generation #{i}/{_generationsCount} ({workDonePercent:N2}%).");
                }

                var fitnessScores = _population.Select(dict => EvaluateFitness(dict)).ToList();
                var bestFitness   = fitnessScores.Max();
                var bestIndex     = fitnessScores.IndexOf(bestFitness);
                double accuracy   = 100.0 * bestFitness / _cipherWords.Count;

                localBestDict = _population[bestIndex];

                if (accuracy > _maxAccuracy)
                {
                    _bestDict    = localBestDict;
                    _maxAccuracy = accuracy;
                    File.WriteAllText("best.txt", accuracy.ToString("N2") + "% -> " + GenerateStringForDict(_bestDict));
                }

                if (shouldLog)
                {
                    ConsoleLog($"Score is {bestFitness}/{_cipherWords.Count} ({accuracy:N2}%).");

                    var  timePassed       = DateTime.Now - startTime;
                    long estimatedMs      = (long)timePassed.TotalMilliseconds * (_generationsCount - i) / i;
                    var  timeEstimated    = TimeSpan.FromMilliseconds(estimatedMs);
                    ConsoleLog($"Time passed: {timePassed}, time estimated: ~{timeEstimated}");
                }

                if (bestFitness == _cipherWords.Count)
                {
                    ConsoleLog("Solution found!");
                    break;
                }

                var selectedCandidates = SelectBestCandidates(_population, _bestCandidatesCount); // Example selection size
                _population = new List<Dictionary<char, char>>();

                while (_population.Count < _populationSize) // Refilling the population
                {
                    var parent1 = selectedCandidates[_random.Next(selectedCandidates.Count)];
                    var parent2 = selectedCandidates[_random.Next(selectedCandidates.Count)];

                    var child = Crossover(parent1, parent2);
                    Mutate(child);
                    _population.Add(child);
                }
            }

            ConsoleLog($"Decoded text: {Decode()}");

            ConsoleLog($"Best fit dict:\n {GenerateStringForDict(_bestDict)}");
        }
    }
}
