# Bonus task A
Your task is to perform cryptanalysis and decrypt the text from the file: **monoalphabetic_cipher_2024.txt**
The source text is in English. You know that monoalphabetic substitution was used for encryption. Yippee
unthinkable to browse all 26! possible solutions using the brute force method. Therefore, use frequency
analysis (take into account individual signs or pairs/triples of signs - so-called bigrams and trigrams,
n-grams in general).
Focus on the words as well, because the task has been simplified so that the spaces were from encryption
excluded (i.e. the gap is in the same place in both the encrypted and the original text). The text contains only
characters of the lowercase alphabet, i.e. no punctuation marks (dots, commas) or numbers. Alphabet, se
which is worked on is just **[a\-z]** + space. End-of-line characters have also been replaced with a space.
Your program must automatically decide that the cipher has been broken, i.e. the resulting text is
meaningful. Use, for example, an English dictionary and test how many correct words you have discovered. What you will be
closer to the correct solution, the number of meaningful words will increase until you gradually get to
maximum.
Also create a *readme.txt* file in which you describe your solution.
Package your program together with the *readme.txt* file and all files from the archive with the entry
into a ZIP file and name it after your personal number.
Help: Frequency analysis, genetic and evolutionary algorithms
You will receive 6 points for a successful solution.

# Solution (C#)
I used a classic genetic algorithm.
Combined many lists of English words for use in the program: the file is sorted, the code uses a HashSet.
StringBuilder is used to concatenate strings, it is much faster than just using string addition.
The scoring function uses the ratio of successfully found words to all words in the text. The difference is that I don't use all the words from the text. I first filtered them and use only unique words in the function.
100% accuracy could not be achieved due to the fact that some non-English words were used in the text (or, for example, there would not be such a large dictionary). There were not so many of them to check them manually, although even without this it was clear that statistically, the chance that the text had already been completely decrypted was incredibly high. The final accuracy of the program was 98.36%.
The results are in the **bin\Debug\net6.0** folder.