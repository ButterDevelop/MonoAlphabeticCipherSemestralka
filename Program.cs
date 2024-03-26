using MonoAlphabeticCipher_Semestralka_Andrei;

const string BEST_SOLUTION_FILENAME  = "best.txt",
             CIPHER_FILENAME         = "monoalphabetic_cipher_2024.txt",
             WORDS_LIST_FILENAME     = "words_united.txt",
             DECODED_RESULT_FILENAME = "result.txt";

var work = new Work(CIPHER_FILENAME, WORDS_LIST_FILENAME);
work.Init();

Console.Write("Write 'solution' to find solution and 'decode' to decode text with the best found solution: ");
string? choice;
while ((choice = Console.ReadLine()) == null || (choice.ToLower() != "solution" && choice.ToLower() != "decode")) Console.WriteLine("Something wrong.");

if (choice.ToLower() == "decode")
{
    string bestSolution = File.Exists(BEST_SOLUTION_FILENAME) ? File.ReadAllText(BEST_SOLUTION_FILENAME) : string.Empty;

    string decoded = work.Decode(bestSolution);
    Console.WriteLine("Decoded with our best solution. Writed to 'result.txt'.");
    File.WriteAllText(DECODED_RESULT_FILENAME, decoded);

    Console.WriteLine("These words were not found in English dictionary: ");
    work.WriteNotFoundWords(bestSolution);
    Console.WriteLine();
}
else
{
    work.FindSolution();
}

Console.ReadKey();