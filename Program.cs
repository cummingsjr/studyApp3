// C# LEARNING COACH: GAME EDITION
// Earn XP, level up, protect your hearts, build streaks, unlock badges,
// and beat the Boss Rush. Your high score is saved to highscore.txt.

Console.OutputEncoding = System.Text.Encoding.UTF8;

const int START_LIVES = 3;
const int XP_PER_LEVEL = 100;
const string SCORE_FILE = "highscore.txt";

int xp = 0, lives = START_LIVES, streak = 0, bestStreak = 0, highScore = 0;
string playerName = "Player";
int[] right = new int[3];
int[] asked = new int[3];
string[] badgeNames = { "First Blood (first correct answer)", "On Fire (5 in a row)", "Bookworm (finish all lessons)", "Boss Slayer (clear Boss Rush)" };
bool[] badges = new bool[4];
bool[] lessonDone = new bool[Content.Steps().Length];
Random rng = new Random();

if (File.Exists(SCORE_FILE))
{
    int.TryParse(File.ReadAllText(SCORE_FILE).Trim(), out highScore);
}

Console.Clear();
Say("=====================================", ConsoleColor.Yellow);
Say("   C# LEARNING COACH: GAME EDITION   ", ConsoleColor.Yellow);
Say("=====================================", ConsoleColor.Yellow);
Console.Write("Enter your player name: ");
string typedName = ReadInput().Trim();
if (typedName != "") playerName = typedName;

while (true)
{
    Console.Clear();
    ShowHud();
    Console.WriteLine("1. Training Grounds   (guided lessons, +5 XP each)");
    Console.WriteLine("2. Quiz Battle        (multiple choice, 3 hearts)");
    Console.WriteLine("3. Code Forge         (type the line)");
    Console.WriteLine("4. Output Oracle      (what does this print?)");
    Console.WriteLine("5. BOSS RUSH          (6 mixed rounds, big XP)");
    Console.WriteLine("6. Player Card        (stats and badges)");
    Console.WriteLine("7. Quit");
    Console.Write("Choose 1-7: ");

    string choice = ReadInput().Trim();

    if (choice == "1") RunLessons();
    else if (choice == "2") RunQuiz();
    else if (choice == "3") RunPractice();
    else if (choice == "4") RunTrace();
    else if (choice == "5") RunBoss();
    else if (choice == "6") ShowCard();
    else if (choice == "7") break;
    else
    {
        Console.WriteLine("Please type a number from 1 to 7.");
        Pause();
    }
}

SaveScore();
Console.WriteLine($"Final XP: {xp}. High score: {highScore}. Keep practicing, {playerName}!");
return;

// --------------------------------------------------
// GAME SYSTEM
// --------------------------------------------------
void Say(string text, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}

int Level()
{
    return xp / XP_PER_LEVEL + 1;
}

string Rank()
{
    int level = Level();

    if (level >= 7) return "Method Master";
    else if (level >= 5) return "Loop Knight";
    else if (level >= 3) return "Array Apprentice";
    else return "Rookie Coder";
}

string Hearts()
{
    string h = "";

    for (int i = 0; i < START_LIVES; i++)
    {
        if (i < lives) h += "♥";
        else h += "♡";
    }

    return h;
}

void ShowHud()
{
    int into = xp % XP_PER_LEVEL;
    int filled = into / 10;
    string bar = new string('#', filled) + new string('.', 10 - filled);

    Say($"{playerName} | Lv {Level()} {Rank()} | XP [{bar}] {into}/{XP_PER_LEVEL}", ConsoleColor.Cyan);
    Say($"Hearts: {Hearts()}   Streak: {streak}   High score: {highScore}", ConsoleColor.Green);
    Console.WriteLine(new string('=', 50));
}

void Unlock(int index)
{
    if (!badges[index])
    {
        badges[index] = true;
        Say($"*** BADGE UNLOCKED: {badgeNames[index]} ***", ConsoleColor.Magenta);
    }
}

// Adds XP (plus a streak bonus) and announces level ups. Returns XP gained.
int AwardXp(int basePoints)
{
    int bonus = Math.Min(streak, 5) * 2;
    int gained = basePoints + bonus;
    int before = Level();

    xp += gained;

    if (xp > highScore)
    {
        highScore = xp;
        SaveScore();
    }

    if (Level() > before)
    {
        Say($"*** LEVEL UP! You are now level {Level()}: {Rank()} ***", ConsoleColor.Yellow);
    }

    return gained;
}

void Correct(int basePoints)
{
    streak++;
    if (streak > bestStreak) bestStreak = streak;

    int gained = AwardXp(basePoints);
    Say($"Correct! +{gained} XP (streak {streak})", ConsoleColor.Green);

    Unlock(0);
    if (streak >= 5) Unlock(1);
}

void Wrong()
{
    streak = 0;
    if (lives > 0) lives--;
    Say($"Ouch! You lost a heart. {Hearts()}", ConsoleColor.Red);
}

void StartRun()
{
    lives = START_LIVES;
    streak = 0;
}

void GameOver()
{
    Say("*** GAME OVER: you ran out of hearts! ***", ConsoleColor.Red);
    Console.WriteLine("Review the lessons and try again.");
    Pause();
}

void Victory(string modeName, int cleared, int total)
{
    int bonus = AwardXp(20);
    Say($"*** {modeName} COMPLETE! {cleared} / {total} correct. Clear bonus +{bonus} XP ***", ConsoleColor.Yellow);
    Pause();
}

void SaveScore()
{
    File.WriteAllText(SCORE_FILE, highScore.ToString());
}

// --------------------------------------------------
// SINGLE-QUESTION ROUTINES
// Each returns 1 = right, 0 = wrong, -1 = player quit.
// --------------------------------------------------
int AskQuestion(Question q, int basePoints)
{
    Console.WriteLine(q.Text);
    Console.WriteLine();

    for (int o = 0; o < q.Options.Length; o++)
    {
        Console.WriteLine($"  {(char)('A' + o)}. {q.Options[o]}");
    }

    Console.Write("Your answer (letter, or q to quit): ");
    string answer = ReadInput().Trim().ToUpper();

    while (answer.Length != 1 ||
           (answer[0] != 'Q' &&
            (answer[0] < 'A' || answer[0] >= 'A' + q.Options.Length)))
    {
        Console.Write("Please type one of the letters shown: ");
        answer = ReadInput().Trim().ToUpper();
    }

    if (answer == "Q") return -1;

    asked[0]++;

    if (answer[0] - 'A' == q.Correct)
    {
        right[0]++;
        Correct(basePoints);
        Console.WriteLine(q.Why);
        return 1;
    }

    Wrong();
    Console.WriteLine($"The answer is {(char)('A' + q.Correct)}.");
    Console.WriteLine(q.Why);
    return 0;
}

int AskExercise(Exercise e, int basePoints)
{
    Console.WriteLine(e.Prompt);
    Console.WriteLine("(hint costs 5 XP, show costs a heart, q quits)");
    Console.WriteLine();

    int tries = 0;
    int points = basePoints;

    while (true)
    {
        Console.Write("> ");
        string line = ReadInput();
        string command = line.Trim().ToLower();

        if (command == "q") return -1;

        if (command == "hint")
        {
            Console.WriteLine("Hint: " + e.Hint);
            points = Math.Max(5, points - 5);
        }
        else if (command == "show")
        {
            asked[1]++;
            Wrong();
            Console.WriteLine("Answer: " + e.Accepted[0]);
            Console.WriteLine(e.Why);
            return 0;
        }
        else if (IsAccepted(line, e.Accepted))
        {
            asked[1]++;
            right[1]++;
            Correct(points);
            Console.WriteLine(e.Why);
            return 1;
        }
        else
        {
            tries++;
            Console.WriteLine("Not quite. Check spelling, capitals, and punctuation.");

            if (tries == 3)
            {
                Console.WriteLine("Type hint for a clue or show for the answer.");
            }
        }
    }
}

int AskTrace(Trace t, int basePoints)
{
    Console.WriteLine("What does this print?");
    Console.WriteLine(new string('-', 50));
    Console.WriteLine(t.Code);
    Console.WriteLine(new string('-', 50));
    Console.Write("Output (q to quit): ");

    string answer = ReadInput();

    if (answer.Trim().ToLower() == "q") return -1;

    asked[2]++;

    if (Normalize(answer) == Normalize(t.Output))
    {
        right[2]++;
        Correct(basePoints);
        Console.WriteLine(t.Why);
        return 1;
    }

    Wrong();
    Console.WriteLine("It prints: " + t.Output);
    Console.WriteLine(t.Why);
    return 0;
}

// --------------------------------------------------
// MODES
// --------------------------------------------------
void RunLessons()
{
    Step[] steps = Content.Steps();

    for (int i = 0; i < steps.Length; i++)
    {
        Console.Clear();
        ShowHud();
        Console.WriteLine($"LESSON {i + 1} of {steps.Length}: {steps[i].Title}");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine(steps[i].Code);
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("WHY IT WORKS:");
        Console.WriteLine(steps[i].Explain);
        Console.WriteLine();

        if (!lessonDone[i])
        {
            lessonDone[i] = true;
            xp += 5;
            if (xp > highScore) highScore = xp;
            Say("+5 XP for studying this lesson!", ConsoleColor.Green);
        }

        Console.Write("Enter = next lesson, q = back to menu: ");

        string next = ReadInput().Trim().ToLower();

        if (next == "q") return;
    }

    bool all = true;
    foreach (bool done in lessonDone)
    {
        if (!done) all = false;
    }

    if (all) Unlock(2);

    Console.WriteLine("You finished all lessons. Try the Quiz Battle next!");
    Pause();
}

void RunQuiz()
{
    Question[] questions = Content.Questions();
    Shuffle(questions);
    StartRun();
    int cleared = 0;

    for (int i = 0; i < questions.Length; i++)
    {
        Console.Clear();
        ShowHud();
        Console.WriteLine($"QUESTION {i + 1} of {questions.Length}");

        int result = AskQuestion(questions[i], 10);

        if (result == -1) return;
        if (result == 1) cleared++;

        if (lives == 0)
        {
            Pause();
            GameOver();
            return;
        }

        Pause();
    }

    Victory("QUIZ BATTLE", cleared, questions.Length);
}

void RunPractice()
{
    Exercise[] exercises = Content.Exercises();
    Shuffle(exercises);
    StartRun();
    int cleared = 0;

    Console.Clear();
    ShowHud();
    Console.WriteLine("CODE FORGE");
    Console.WriteLine("Type the line of C# that does the job.");
    Console.WriteLine("Spacing does not matter, but spelling and capital letters do.");
    Pause();

    for (int i = 0; i < exercises.Length; i++)
    {
        Console.Clear();
        ShowHud();
        Console.WriteLine($"EXERCISE {i + 1} of {exercises.Length}");

        int result = AskExercise(exercises[i], 15);

        if (result == -1) return;
        if (result == 1) cleared++;

        if (lives == 0)
        {
            Pause();
            GameOver();
            return;
        }

        Pause();
    }

    Victory("CODE FORGE", cleared, exercises.Length);
}

void RunTrace()
{
    Trace[] traces = Content.Traces();
    Shuffle(traces);
    StartRun();
    int cleared = 0;

    for (int i = 0; i < traces.Length; i++)
    {
        Console.Clear();
        ShowHud();
        Console.WriteLine($"TRACE {i + 1} of {traces.Length}");

        int result = AskTrace(traces[i], 15);

        if (result == -1) return;
        if (result == 1) cleared++;

        if (lives == 0)
        {
            Pause();
            GameOver();
            return;
        }

        Pause();
    }

    Victory("OUTPUT ORACLE", cleared, traces.Length);
}

void RunBoss()
{
    Question[] questions = Content.Questions();
    Exercise[] exercises = Content.Exercises();
    Trace[] traces = Content.Traces();
    Shuffle(questions);
    Shuffle(exercises);
    Shuffle(traces);
    StartRun();

    int rounds = 6;
    int cleared = 0;

    Console.Clear();
    Say("!!! BOSS RUSH !!!", ConsoleColor.Red);
    Console.WriteLine("Six rounds mixing quiz, code, and trace questions.");
    Console.WriteLine("Every correct answer is worth extra XP. Three hearts. Good luck!");
    Pause();

    for (int round = 0; round < rounds; round++)
    {
        Console.Clear();
        ShowHud();
        Say($"BOSS ROUND {round + 1} of {rounds}", ConsoleColor.Red);

        int kind = round % 3;
        int result;

        if (kind == 0) result = AskQuestion(questions[round / 3], 20);
        else if (kind == 1) result = AskExercise(exercises[round / 3], 20);
        else result = AskTrace(traces[round / 3], 20);

        if (result == -1) return;
        if (result == 1) cleared++;

        if (lives == 0)
        {
            Pause();
            GameOver();
            return;
        }

        Pause();
    }

    if (cleared >= 4) Unlock(3);

    Victory("BOSS RUSH", cleared, rounds);
}

void ShowCard()
{
    string[] names = { "Quiz Battle", "Code Forge", "Output Oracle" };

    Console.Clear();
    ShowHud();
    Console.WriteLine("=== PLAYER CARD ===");
    Console.WriteLine($"Name:        {playerName}");
    Console.WriteLine($"Rank:        {Rank()} (level {Level()})");
    Console.WriteLine($"Total XP:    {xp}");
    Console.WriteLine($"Best streak: {bestStreak}");
    Console.WriteLine();

    for (int i = 0; i < names.Length; i++)
    {
        Console.WriteLine(names[i].PadRight(18) + $"{right[i]} / {asked[i]}");
    }

    int totalRight = right[0] + right[1] + right[2];
    int totalAsked = asked[0] + asked[1] + asked[2];
    Console.WriteLine("Total".PadRight(18) + $"{totalRight} / {totalAsked}");

    Console.WriteLine();
    Console.WriteLine("BADGES:");

    for (int i = 0; i < badgeNames.Length; i++)
    {
        if (badges[i]) Say("  [X] " + badgeNames[i], ConsoleColor.Magenta);
        else Console.WriteLine("  [ ] " + badgeNames[i]);
    }

    Pause();
}

// --------------------------------------------------
// HELPERS
// --------------------------------------------------
// Reads a line; if the input stream ends, exit instead of looping forever.
string ReadInput()
{
    string? line = Console.ReadLine();

    if (line == null) Environment.Exit(0);

    return line;
}

void Pause()
{
    Console.Write("Press Enter to continue...");
    ReadInput();
}

void Shuffle<T>(T[] items)
{
    for (int i = items.Length - 1; i > 0; i--)
    {
        int j = rng.Next(i + 1);
        (items[i], items[j]) = (items[j], items[i]);
    }
}

// Removes all spaces and a trailing semicolon so formatting
// differences do not count as mistakes.
string Normalize(string text)
{
    string result = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());

    if (result.EndsWith(";")) result = result.Substring(0, result.Length - 1);

    return result;
}

bool IsAccepted(string line, string[] accepted)
{
    string typed = Normalize(line);

    foreach (string a in accepted)
    {
        if (typed == Normalize(a)) return true;
    }

    return false;
}

// --------------------------------------------------
// TYPES
// --------------------------------------------------
record Step(string Title, string Code, string Explain);
record Question(string Text, string[] Options, int Correct, string Why);
record Exercise(string Prompt, string[] Accepted, string Hint, string Why);
record Trace(string Code, string Output, string Why);

static class Content
{
    public static Step[] Steps() => new[]
    {
        new Step("Constants",
            "const string PLAYERS_FILE = \"players.txt\";\nconst int MAX_POINTS = 20;",
            "A const is a named value that can never change. Naming 20 as MAX_POINTS means\nyou change it in one place, and the code reads like English."),

        new Step("Reading a file into an array",
            "string[] players = File.ReadAllLines(PLAYERS_FILE);",
            "File.ReadAllLines returns one string per line of the file.\nplayers[0] is the first line, players.Length is how many lines there are."),

        new Step("Parallel arrays and the for loop",
            "for (int i = 0; i < players.Length; i++)\n{\n    Console.WriteLine(players[i].PadRight(11) + points[i]);\n}",
            "players and points line up: index 3 in both belongs to the same person.\nA for loop gives you that shared index i. It starts at 0 and stops before\nLength because the last valid index is Length - 1."),

        new Step("foreach",
            "foreach (string entry in history)\n{\n    Console.WriteLine(\"  \" + entry);\n}",
            "Use foreach when you only need each item and not its position.\nIt is shorter and you cannot get the index wrong."),

        new Step("Methods",
            "static int TotalPoints(string[] points)\n{\n    int total = 0;\n    foreach (string point in points)\n        total += Convert.ToInt32(point);\n    return total;\n}",
            "static int means: a method that returns an int. The part in parentheses is the\nparameter list. The method does its own work, then return sends the answer back.\nCall it with: int seasonTotal = TotalPoints(points);"),

        new Step("Converting between text and numbers",
            "int season = Convert.ToInt32(points[i]) + tonight;\npoints[i] = season.ToString();",
            "File lines are strings, and \"10\" + \"5\" would give \"105\". Convert.ToInt32\nturns text into a number so math works. ToString turns it back for saving."),

        new Step("while for input validation",
            "while (tonight < 0 || tonight > MAX_POINTS)\n{\n    Console.WriteLine(\"Points must be from 0 to 20.\");\n    tonight = Convert.ToInt32(Console.ReadLine());\n}",
            "A while loop repeats as long as its condition is true. Here it keeps asking\nuntil the score is valid. || means OR: bad if below 0 OR above the max."),

        new Step("Tracking a best value with if",
            "if (tonight > bestTonight)\n{\n    bestTonight = tonight;\n    winnerTonight = players[i];\n}",
            "Start bestTonight at -1 so any real score beats it. Each time a score is\nhigher, remember both the score and who earned it."),

        new Step("Saving to files",
            "File.WriteAllLines(POINTS_FILE, points);\nFile.AppendAllLines(HISTORY_FILE, newEntry);",
            "WriteAllLines replaces the whole file, which is right for the standings.\nAppendAllLines adds to the end, which is right for a growing history.\nBoth take an array (or list) of lines."),

        new Step("Formatting text",
            "$\"Leader: {players[leader]} with {points[leader]}\"\n\"PLAYER\".PadRight(11) + \"POINTS\"",
            "A string starting with $ is interpolated: anything inside { } is evaluated.\nPadRight(11) adds spaces until the text is 11 characters wide, so columns line up."),
    };

    public static Question[] Questions() => new[]
    {
        new Question("What does File.ReadAllLines(\"x.txt\") return?",
            new[] { "One big string", "A string[] with one item per line", "An int", "A list of numbers" },
            1, "Each line of the file becomes one element of a string array."),

        new Question("An array has 5 items. What is the last valid index?",
            new[] { "5", "6", "4", "0" },
            2, "Indexes start at 0, so the last one is Length - 1 = 4."),

        new Question("Which loop is best when you need the position of each item?",
            new[] { "foreach", "for", "if", "const" },
            1, "A for loop gives you the index variable i."),

        new Question("What is the value of \"7\" + \"3\" in C#?",
            new[] { "10", "\"73\"", "\"10\"", "An error" },
            1, "Plus on two strings joins them. Convert them to int first to add."),

        new Question("What does static int Add(int a, int b) promise?",
            new[] { "It prints an int", "It returns an int", "It changes a and b", "It has no parameters" },
            1, "The type before the name (int) is what the method returns."),

        new Question("Which condition means 'score is invalid' for a 0-20 range?",
            new[] { "score < 0 && score > 20", "score < 0 || score > 20", "score > 0 || score < 20", "score == 20" },
            1, "A score is bad if it is below 0 OR above 20. && would never be true."),

        new Question("What does File.AppendAllLines do?",
            new[] { "Replaces the file", "Deletes the file", "Adds lines to the end of the file", "Reads the file" },
            2, "Append means add to the end, keeping what was already there."),

        new Question("Why start 'best' at -1 when tracking the highest score of 0-20?",
            new[] { "-1 is lucky", "So any real score is bigger", "Arrays start at -1", "C# requires it" },
            1, "Starting below every possible score guarantees the first score replaces it."),

        new Question("What does 7 / 2 give when both are ints?",
            new[] { "3.5", "4", "3", "2" },
            2, "Integer division drops the remainder. Use 7 % 2 to get the remainder (1)."),

        new Question("When should you choose foreach over for?",
            new[] { "When you need the index", "When you only need each item", "When looping backwards", "Never" },
            1, "foreach is simplest when the position does not matter."),

        new Question("What does $\"Hi {name}\" do?",
            new[] { "Prints {name} literally", "Inserts the value of name into the text", "Declares a variable", "Causes an error" },
            1, "The $ turns on interpolation, so names inside { } are replaced by their values."),

        new Question("Which keyword makes a value that can never change?",
            new[] { "static", "const", "return", "void" },
            1, "const creates a constant, like const int MAX_POINTS = 20;"),
    };

    public static Exercise[] Exercises() => new[]
    {
        new Exercise("Declare an int constant named MAX_POINTS equal to 20.",
            new[] { "const int MAX_POINTS = 20;" },
            "Start with const, then the type, the name, = and the value.",
            "const makes the value permanent and the ALL_CAPS name signals that."),

        new Exercise("Read every line of the file named by PLAYERS_FILE into a string array called players.",
            new[] { "string[] players = File.ReadAllLines(PLAYERS_FILE);" },
            "The method is File.ReadAllLines and it returns a string[].",
            "ReadAllLines returns one array element per line."),

        new Exercise("Write the for loop header that runs i from 0 through every index of players.",
            new[] { "for (int i = 0; i < players.Length; i++)" },
            "Three parts: int i = 0; then i < players.Length; then i++",
            "Use < (not <=) with Length because the last index is Length - 1."),

        new Exercise("Write a foreach header that loops over history, naming each item entry (a string).",
            new[] { "foreach (string entry in history)" },
            "foreach (type name in collection)",
            "foreach hands you each item directly, with no index."),

        new Exercise("Convert the string points[i] to an int and store it in a variable called value.",
            new[] { "int value = Convert.ToInt32(points[i]);", "int value = int.Parse(points[i]);" },
            "Convert.ToInt32( ... )",
            "File data is text, so it must be converted before doing math."),

        new Exercise("Write the while header that repeats while tonight is below 0 or above MAX_POINTS.",
            new[] { "while (tonight < 0 || tonight > MAX_POINTS)" },
            "Use || between the two comparisons.",
            "This loop only runs when the value is invalid, so it re-asks until valid."),

        new Exercise("Write the signature (first line only) of a static method named TotalPoints that returns an int and takes a string[] named points.",
            new[] { "static int TotalPoints(string[] points)" },
            "static  return-type  Name(parameter-type name)",
            "The signature tells callers the name, the inputs, and the output type."),

        new Exercise("Add the lines in newEntry to the end of the file named by HISTORY_FILE.",
            new[] { "File.AppendAllLines(HISTORY_FILE, newEntry);" },
            "Append... not Write...",
            "AppendAllLines keeps the old history and adds the new line."),

        new Exercise("Print the text Leader: followed by the value of players[leader], using an interpolated string.",
            new[] { "Console.WriteLine($\"Leader: {players[leader]}\");" },
            "Console.WriteLine($\"text {expression}\");",
            "The $ before the quote lets you place variables inside { }."),

        new Exercise("Declare an int called total and set it to 0.",
            new[] { "int total = 0;" },
            "type name = value;",
            "Totals and counters should start at 0 before you add to them."),
    };

    public static Trace[] Traces() => new[]
    {
        new Trace("int total = 0;\nforeach (string p in new[] { \"4\", \"6\", \"10\" })\n    total += Convert.ToInt32(p);\nConsole.WriteLine(total);",
            "20", "4 + 6 + 10 = 20."),

        new Trace("string s = \"7\";\nstring t = \"5\";\nConsole.WriteLine(s + t);",
            "75", "These are strings, so + joins them instead of adding."),

        new Trace("for (int i = 1; i < 4; i++)\n    Console.Write(i + \" \");",
            "1 2 3", "i starts at 1 and stops once i < 4 is false, so 4 is never printed."),

        new Trace("int[] a = { 3, 9, 4 };\nint best = 0;\nfor (int i = 1; i < a.Length; i++)\n    if (a[i] > a[best]) best = i;\nConsole.WriteLine(best);",
            "1", "It stores the INDEX of the biggest value. 9 is at index 1."),

        new Trace("int x = 5;\nwhile (x < 20)\n    x += 6;\nConsole.WriteLine(x);",
            "23", "x goes 5, 11, 17, 23. At 23 the condition is false so the loop stops."),

        new Trace("int n = 7 / 2;\nint r = 7 % 2;\nConsole.WriteLine(n + \" \" + r);",
            "3 1", "Integer division gives 3 and % gives the remainder 1."),

        new Trace("string[] names = { \"Ana\", \"Bo\", \"Cy\" };\nConsole.WriteLine(names[names.Length - 1]);",
            "Cy", "Length is 3, so the last index is 2, which holds Cy."),
    };
}

