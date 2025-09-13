using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

/// <summary>
/// Программа для конвертации изображения в ASCII-графику и обратно, с поддержкой оттенков серого, цвета, дизеринга, настройки яркости/контраста, улучшения качества и сохранения в различных форматах (TXT, HTML, PNG).
/// Program for converting images to ASCII graphics and back, supporting grayscale shades, color, dithering, brightness/contrast adjustment, quality enhancement, and saving in various formats (TXT, HTML, PNG).
/// </summary>
public class AsciiArtGenerator
{
    // Палитра символов для градации яркости от темного к светлому.
    // Palette of characters for brightness gradation from dark to light.
    private static readonly string[] DefaultAsciiCharacters = { "#", "@", "%", "=", "+", ";", ":", "-", ".", " " };

    // Матрица Байера для дизеринга.
    // Bayer matrix for dithering.
    private static readonly int[,] BayerMatrix = new int[,]
    {
        { 0, 8, 2, 10 },
        { 12, 4, 14, 6 },
        { 3, 11, 1, 9 },
        { 15, 7, 13, 5 }
    };

    // Продвинутая палитра символов для режима "super", отсортированная по плотности.
    // Этот набор включает символы, которые дают более высокую детализацию.
    // Advanced palette of characters for "super" mode, sorted by density.
    // This set includes characters that provide higher detail.
    private static readonly char[] SuperAsciiCharacters = GetSuperAsciiCharacters();

    /// <summary>
    /// Динамически генерирует расширенный набор символов Юникода.
    /// Символы расположены в порядке увеличения "плотности" или "заполненности".
    /// Dynamically generates an extended set of Unicode characters.
    /// Characters are arranged in order of increasing "density" or "fill".
    /// </summary>
    private static char[] GetSuperAsciiCharacters()
    {
        var chars = new List<char>();

        // Самые "пустые" символы
        // The most "empty" characters
        chars.AddRange(new[] { ' ', '.', ',', '\'', '`', ':', ';', '!', 'i', 't', 'l', 'j' });
        // Символы с низкой плотностью
        // Characters with low density
        chars.AddRange(new[] { '-', '_', '^', '/', '\\', '(', ')', '[', ']', '{', '}', '<', '>', '~', '\"' });
        // Символы со средней плотностью
        // Characters with medium density
        chars.AddRange(new[] { '+', '=', 's', 'r', 'n', 'u', 'v', 'c', 'o', 'x', 'z', 'a', 'e', 'E', 'F', 'P', 'p' });
        // Символы с высокой плотностью
        // Characters with high density
        chars.AddRange(new[] { 'b', 'd', 'q', 'g', 'h', 'k', 'y', 'B', 'D', 'O', 'Q', 'G', 'H', 'K', 'S', 'M', 'W' });
        // Самые "плотные" символы, включая блоки и специальные знаки
        // The most "dense" characters, including blocks and special signs
        chars.AddRange(new[] { 'N', 'M', 'W', 'V', 'Z', 'X', 'C', 'L', '#', '@', '%', '&' });
        chars.AddRange(new[] { '█', '▓', '▒', '░' });

        // Дополнительные символы Юникода для более плавной градации
        // Additional Unicode characters for smoother gradation
        for (int i = 9617; i <= 9619; i++) chars.Add((char)i); // Заштрихованные блоки / Shaded blocks
        for (int i = 9600; i <= 9631; i++) chars.Add((char)i); // Символы блоков / Block symbols
        for (int i = 9472; i <= 9599; i++) chars.Add((char)i); // Рисование рамок / Box drawing

        return chars.Distinct().ToArray();
    }

    /// <summary>
    /// Главный метод программы, обрабатывающий аргументы и запускающий соответствующий режим.
    /// Main method of the program, handling arguments and starting the appropriate mode.
    /// </summary>
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (args.Length > 0)
        {
            // Неинтерактивный режим
            // Non-interactive mode
            ProcessArgs(args);
        }
        else
        {
            // Интерактивный режим
            // Interactive mode
            StartInteractiveMode();
        }
    }

    /// <summary>
    /// Запускает нелинейный интерактивный режим с запросом параметров у пользователя.
    /// Starts nonlinear interactive mode with user prompts for parameters.
    /// </summary>
    private static void StartInteractiveMode()
    {
        string inputPath;
        string fileType;
        while (true)
        {
            Console.WriteLine("Пожалуйста, введите полный путь к файлу (.txt, .jpg, .png и т.д.):");
            // Please enter the full path to the file (.txt, .jpg, .png, etc.):
            inputPath = Console.ReadLine();
            if (File.Exists(inputPath))
            {
                fileType = GetFileType(inputPath);
                if (fileType != "unknown")
                {
                    break;
                }
                Console.WriteLine("Ошибка: Неподдерживаемый тип файла. Попробуйте другой файл.");
                // Error: Unsupported file type. Try another file.
            }
            Console.WriteLine("Ошибка: Файл не найден. Попробуйте еще раз.");
            // Error: File not found. Try again.
        }

        string[] args = new string[20]; // Увеличиваем размер для гибкости
        // Increasing size for flexibility
        int argIndex = 0;
        args[argIndex++] = inputPath;

        Console.WriteLine($"\nТип файла определен как: {fileType}");
        // File type determined as: {fileType}
        Console.WriteLine("\nТеперь выберите, что вы хотите сделать. Введите пустую строку, чтобы начать генерацию.");
        // Now choose what you want to do. Enter an empty string to start generation.
        Console.WriteLine("Доступные параметры:");
        // Available parameters:

        // Выводим опции в зависимости от типа файла
        // Output options depending on file type
        if (fileType == "text")
        {
            Console.WriteLine("  -j            Преобразовать текстовый документ в изображение (.png)");
            //  -j            Convert text document to image (.png)
            Console.WriteLine("  -o <путь>     Путь для сохранения результата (по умолчанию: автоматическое сохранение)");
            //  -o <path>     Path to save the result (default: automatic saving)
        }
        else // Image
        {
            Console.WriteLine("  -s <число>    Коэффициент масштабирования (по умолчанию: 4)");
            //  -s <number>   Scaling factor (default: 4)
            Console.WriteLine("  -o <путь>     Путь для сохранения результата (по умолчанию: автоматическое сохранение)");
            //  -o <path>     Path to save the result (default: automatic saving)
            Console.WriteLine("  -p <строка>   Кастомная палитра символов (например: -p \"#*o. \")");
            //  -p <string>   Custom character palette (e.g.: -p "#*o. ")
            Console.WriteLine("  -c            Включить цветной режим");
            //  -c            Enable color mode
            Console.WriteLine("  -d <тип>      Включить дизеринг (floyd-steinberg, bayer, advanced или super)");
            //  -d <type>     Enable dithering (floyd-steinberg, bayer, advanced or super)
            Console.WriteLine("  -b <число>    Изменить яркость (от -100 до 100)");
            //  -b <number>   Change brightness (from -100 to 100)
            Console.WriteLine("  -k <число>    Изменить контраст (от -100 до 100)");
            //  -k <number>   Change contrast (from -100 to 100)
            Console.WriteLine("  -i            Инвертировать цвета");
            //  -i            Invert colors
            Console.WriteLine("  -q            Улучшить качество (применить фильтр резкости)");
            //  -q            Enhance quality (apply sharpening filter)
            Console.WriteLine("  -h            Экспорт в HTML-файл");
            //  -h            Export to HTML file
            Console.WriteLine("  -u            Создать полноцветный символьный портрет (.png)");
            //  -u            Create full-color character portrait (.png)
        }

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            string[] inputArgs = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (inputArgs.Length == 0) continue;

            for (int i = 0; i < inputArgs.Length; i++)
            {
                if (argIndex < args.Length)
                {
                    args[argIndex++] = inputArgs[i];
                }
            }
        }

        Array.Resize(ref args, argIndex);
        ProcessArgs(args);
    }

    /// <summary>
    /// Определяет тип файла по его расширению.
    /// Determines the file type by its extension.
    /// </summary>
    private static string GetFileType(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        switch (extension)
        {
            case ".txt":
                return "text";
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".bmp":
            case ".tiff":
                return "image";
            default:
                return "unknown";
        }
    }

    /// <summary>
    /// Обрабатывает аргументы командной строки и запускает соответствующую функцию.
    /// Processes command-line arguments and launches the corresponding function.
    /// </summary>
    private static void ProcessArgs(string[] args)
    {
        string inputPath = args[0];
        string fileType = GetFileType(inputPath);

        int scaleFactor = 4;
        string outputPath = string.Empty;
        string[] asciiChars = DefaultAsciiCharacters;
        bool useColor = false;
        string ditheringType = string.Empty;
        int brightness = 0;
        int contrast = 0;
        bool invertColors = false;
        bool enhanceQuality = false;

        // Новая переменная для отслеживания режима вывода, чтобы избежать конфликтов.
        // New variable to track output mode to avoid conflicts.
        string outputMode = "default";

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-s":
                    if (fileType != "text" && i + 1 < args.Length && int.TryParse(args[i + 1], out int s)) { scaleFactor = s; i++; }
                    break;
                case "-o":
                    if (i + 1 < args.Length) { outputPath = args[i + 1]; i++; }
                    break;
                case "-p":
                    if (fileType != "text" && i + 1 < args.Length)
                    {
                        asciiChars = args[i + 1].ToCharArray().Select(c => c.ToString()).ToArray();
                        Array.Reverse(asciiChars);
                        i++;
                    }
                    break;
                case "-c":
                    if (fileType != "text") useColor = true;
                    outputMode = "text";
                    break;
                case "-d":
                    if (fileType != "text" && i + 1 < args.Length) { ditheringType = args[i + 1].ToLower(); i++; }
                    outputMode = "text";
                    break;
                case "-b":
                    if (fileType != "text" && i + 1 < args.Length && int.TryParse(args[i + 1], out int b)) { brightness = Math.Max(-100, Math.Min(100, b)); i++; }
                    break;
                case "-k":
                    if (fileType != "text" && i + 1 < args.Length && int.TryParse(args[i + 1], out int k)) { contrast = Math.Max(-100, Math.Min(100, k)); i++; }
                    break;
                case "-i":
                    if (fileType != "text") invertColors = true;
                    break;
                case "-q":
                    if (fileType != "text") enhanceQuality = true;
                    break;
                case "-h":
                    if (fileType != "text") outputMode = "html";
                    break;
                case "-j":
                    if (fileType == "text") outputMode = "text_to_image";
                    break;
                case "-u":
                    if (fileType != "text") outputMode = "universal_png";
                    break;
            }
        }

        try
        {
            switch (fileType)
            {
                case "image":
                    ProcessImage(inputPath, scaleFactor, outputPath, asciiChars, useColor, ditheringType, brightness, contrast, invertColors, enhanceQuality, outputMode);
                    break;
                case "text":
                    if (outputMode == "text_to_image")
                    {
                        ConvertTextToImage(inputPath, outputPath);
                    }
                    else
                    {
                        ProcessText(inputPath, outputPath);
                    }
                    break;
                default:
                    Console.WriteLine("Ошибка: Неподдерживаемый тип файла. На данный момент поддерживаются только изображения и текстовые файлы.");
                    // Error: Unsupported file type. Currently, only images and text files are supported.
                    break;
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            Console.WriteLine($"Ошибка: Файл не найден по указанному пути: {inputPath}");
            // Error: File not found at the specified path: {inputPath}
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла непредвиденная ошибка: {ex.Message}");
            // An unexpected error occurred: {ex.Message}
        }
    }

    /// <summary>
    /// Простая обработка текстового файла без преобразования, копирует содержимое в новый файл.
    /// Simple processing of a text file without conversion, copies the content to a new file.
    /// </summary>
    private static void ProcessText(string textPath, string outputPath)
    {
        string content = File.ReadAllText(textPath);
        string finalOutputPath = outputPath;
        if (string.IsNullOrEmpty(finalOutputPath))
        {
            finalOutputPath = Path.Combine(Path.GetDirectoryName(textPath), "output_" + Path.GetFileNameWithoutExtension(textPath) + ".txt");
        }
        File.WriteAllText(finalOutputPath, content);
        Console.WriteLine($"Текст успешно скопирован и сохранен в файл: {finalOutputPath}");
        // Text successfully copied and saved to file: {finalOutputPath}
    }

    /// <summary>
    /// Преобразует текстовый файл в изображение (.png), рендерит текст на белом фоне с черным шрифтом.
    /// Converts a text file to an image (.png), renders text on a white background with black font.
    /// </summary>
    private static void ConvertTextToImage(string textPath, string outputPath)
    {
        string[] lines = File.ReadAllLines(textPath);
        if (lines.Length == 0)
        {
            Console.WriteLine("Ошибка: Текстовый файл пуст.");
            // Error: Text file is empty.
            return;
        }

        int longestLineLength = lines.Max(l => l.Length);

        using (var font = new Font("Courier New", 12))
        using (var tempBitmap = new Bitmap(1, 1))
        using (var graphics = Graphics.FromImage(tempBitmap))
        {
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            SizeF stringSize = graphics.MeasureString(new string('W', longestLineLength), font);

            int imageWidth = (int)Math.Ceiling(stringSize.Width);
            int imageHeight = (int)Math.Ceiling(stringSize.Height * lines.Length);

            using (var bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb))
            using (var textGraphics = Graphics.FromImage(bitmap))
            {
                textGraphics.Clear(Color.White);
                textGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                using (var brush = new SolidBrush(Color.Black))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        textGraphics.DrawString(lines[i], font, brush, 0, i * font.GetHeight());
                    }
                }

                string finalOutputPath = outputPath;
                if (string.IsNullOrEmpty(finalOutputPath))
                {
                    finalOutputPath = Path.Combine(Path.GetDirectoryName(textPath), "text_" + Path.GetFileNameWithoutExtension(textPath) + ".png");
                }

                bitmap.Save(finalOutputPath, ImageFormat.Png);
                Console.WriteLine($"Текст успешно преобразован в изображение и сохранен в файл: {finalOutputPath}");
                // Text successfully converted to image and saved to file: {finalOutputPath}
            }
        }
    }

    /// <summary>
    /// Обрабатывает изображение, используя параметры, включая масштабирование, дизеринг, корректировку и различные режимы вывода.
    /// Processes the image using parameters, including scaling, dithering, adjustments, and various output modes.
    /// </summary>
    private static void ProcessImage(string imagePath, int scaleFactor, string outputPath, string[] asciiChars, bool useColor, string ditheringType, int brightness, int contrast, bool invertColors, bool enhanceQuality, string outputMode)
    {
        using (var originalImage = new Bitmap(imagePath))
        {
            Console.WriteLine($"Изображение '{Path.GetFileName(imagePath)}' загружено. Преобразование в ASCII-графику...");
            // Image '{Path.GetFileName(imagePath)}' loaded. Converting to ASCII graphics...

            int newWidth = originalImage.Width / scaleFactor;
            int newHeight = originalImage.Height / (scaleFactor * 2);

            // Заменили using-блок на обычное объявление, чтобы избежать ошибки
            // Replaced using block with regular declaration to avoid error
            Bitmap scaledImage = new Bitmap(newWidth, newHeight);

            try
            {
                using (var graphics = Graphics.FromImage(scaledImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                }

                // Применение фильтра резкости, если указан параметр -q
                // Applying sharpening filter if -q parameter is specified
                if (enhanceQuality)
                {
                    Bitmap enhancedImage = EnhanceImageQuality(scaledImage);
                    scaledImage.Dispose();
                    scaledImage = enhancedImage;
                }

                if (invertColors) InvertImageColors(scaledImage);
                if (brightness != 0 || contrast != 0) AdjustImage(scaledImage, brightness, contrast);

                string asciiArt;
                string finalOutputPath;

                switch (outputMode)
                {
                    case "html":
                        string htmlContent = GenerateHtml(scaledImage, asciiChars, ditheringType);
                        finalOutputPath = outputPath == string.Empty ? Path.Combine(Path.GetDirectoryName(imagePath), "ascii_" + Path.GetFileNameWithoutExtension(imagePath) + ".html") : outputPath;
                        File.WriteAllText(finalOutputPath, htmlContent);
                        Console.WriteLine($"Результат успешно сохранен в файл: {finalOutputPath}");
                        // Result successfully saved to file: {finalOutputPath}
                        break;
                    case "universal_png":
                        Bitmap universalImage = ConvertToUniversalImage(scaledImage);
                        finalOutputPath = outputPath == string.Empty ? Path.Combine(Path.GetDirectoryName(imagePath), "ascii_portrait_" + Path.GetFileNameWithoutExtension(imagePath) + ".png") : outputPath;
                        universalImage.Save(finalOutputPath, ImageFormat.Png);
                        Console.WriteLine($"Режим 'универсального портрета' обработан. Результат успешно сохранен в файл: {finalOutputPath}");
                        // 'Universal portrait' mode processed. Result successfully saved to file: {finalOutputPath}
                        break;
                    case "text":
                    default:
                        if (ditheringType == "floyd-steinberg") asciiArt = ConvertToAsciiWithFloydSteinbergDithering(scaledImage, asciiChars);
                        else if (ditheringType == "bayer") asciiArt = ConvertToAsciiWithBayerDithering(scaledImage, asciiChars);
                        else if (ditheringType == "advanced") asciiArt = ConvertToAdvancedAscii(scaledImage, asciiChars);
                        else if (ditheringType == "super") asciiArt = ConvertToSuperAscii(scaledImage);
                        else if (useColor) asciiArt = ConvertToColorAscii(scaledImage, asciiChars);
                        else asciiArt = ConvertToAscii(scaledImage, asciiChars);

                        finalOutputPath = outputPath == string.Empty ? Path.Combine(Path.GetDirectoryName(imagePath), "ascii_" + Path.GetFileNameWithoutExtension(imagePath) + ".txt") : outputPath;
                        File.WriteAllText(finalOutputPath, asciiArt);
                        Console.WriteLine($"Результат успешно сохранен в файл: {finalOutputPath}");
                        // Result successfully saved to file: {finalOutputPath}

                        Console.OutputEncoding = Encoding.UTF8;
                        Console.WriteLine("--- Результат преобразования ---");
                        // --- Conversion result ---
                        Console.WriteLine(asciiArt);
                        Console.WriteLine("--- Конец ---");
                        // --- End ---
                        break;
                }
            }
            finally
            {
                // Убедимся, что scaledImage всегда удаляется
                // Ensure that scaledImage is always disposed
                if (scaledImage != null)
                {
                    scaledImage.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Конвертирует изображение в ASCII-графику, используя палитру символов на основе яркости пикселей.
    /// Converts the image to ASCII graphics using a character palette based on pixel brightness.
    /// </summary>
    private static string ConvertToAscii(Bitmap image, string[] asciiChars)
    {
        StringBuilder resultBuilder = new StringBuilder();
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                int index = (int)(brightness / 255.0 * (asciiChars.Length - 1));
                resultBuilder.Append(asciiChars[index]);
            }
            resultBuilder.Append('\n');
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Конвертирует изображение в цветную ASCII-графику с использованием HTML-стилей для цветов.
    /// Converts the image to color ASCII graphics using HTML styles for colors.
    /// </summary>
    private static string ConvertToColorAscii(Bitmap image, string[] asciiChars)
    {
        StringBuilder resultBuilder = new StringBuilder();
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                int index = (int)(brightness / 255.0 * (asciiChars.Length - 1));

                string hexColor = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}";
                string charToUse = asciiChars[index];

                resultBuilder.Append($"<span style=\"color:{hexColor};\">{charToUse}</span>");
            }
            resultBuilder.Append('\n');
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Применяет дизеринг Флойд-Стейнберга и конвертирует изображение в ASCII-графику.
    /// Applies Floyd-Steinberg dithering and converts the image to ASCII graphics.
    /// </summary>
    private static string ConvertToAsciiWithFloydSteinbergDithering(Bitmap image, string[] asciiChars)
    {
        Bitmap ditheredImage = (Bitmap)image.Clone();
        StringBuilder resultBuilder = new StringBuilder();

        for (int y = 0; y < ditheredImage.Height; y++)
        {
            for (int x = 0; x < ditheredImage.Width; x++)
            {
                Color oldPixel = ditheredImage.GetPixel(x, y);
                int oldGray = (int)(0.299 * oldPixel.R + 0.587 * oldPixel.G + 0.114 * oldPixel.B);
                int newGray = oldGray < 128 ? 0 : 255;

                ditheredImage.SetPixel(x, y, Color.FromArgb(newGray, newGray, newGray));

                int error = oldGray - newGray;

                if (x + 1 < ditheredImage.Width) SpreadError(ditheredImage, x + 1, y, error, 7, 16);
                if (x - 1 >= 0 && y + 1 < ditheredImage.Height) SpreadError(ditheredImage, x - 1, y + 1, error, 3, 16);
                if (y + 1 < ditheredImage.Height) SpreadError(ditheredImage, x, y + 1, error, 5, 16);
                if (x + 1 < ditheredImage.Width && y + 1 < ditheredImage.Height) SpreadError(ditheredImage, x + 1, y + 1, error, 1, 16);
            }
        }

        string result = ConvertToAscii(ditheredImage, asciiChars);
        ditheredImage.Dispose();
        return result;
    }

    /// <summary>
    /// Применяет дизеринг по матрице Байера и конвертирует изображение в ASCII-графику.
    /// Applies Bayer matrix dithering and converts the image to ASCII graphics.
    /// </summary>
    private static string ConvertToAsciiWithBayerDithering(Bitmap image, string[] asciiChars)
    {
        StringBuilder resultBuilder = new StringBuilder();
        int bayerSize = 4;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                double threshold = (BayerMatrix[x % bayerSize, y % bayerSize] + 1.0) / (bayerSize * bayerSize);

                string charToUse = (brightness / 255.0 > threshold) ? asciiChars[0] : asciiChars.Last();
                resultBuilder.Append(charToUse);
            }
            resultBuilder.Append('\n');
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Преобразует изображение в ASCII-графику с помощью продвинутого алгоритма, подбирая символы по соответствию яркостным паттернам блоков.
    /// Converts the image to ASCII graphics using an advanced algorithm, matching characters to brightness patterns in blocks.
    /// </summary>
    private static string ConvertToAdvancedAscii(Bitmap image, string[] asciiChars)
    {
        // Создаем библиотеку шаблонов символов
        // Create a library of character patterns
        var charPatterns = new Dictionary<string, double[,]>();
        foreach (var c in asciiChars)
        {
            var pattern = GenerateCharPattern(c);
            charPatterns[c] = pattern;
        }

        StringBuilder resultBuilder = new StringBuilder();

        int charWidth = 8;
        int charHeight = 8;

        for (int y = 0; y < image.Height - charHeight; y += charHeight)
        {
            for (int x = 0; x < image.Width - charWidth; x += charWidth)
            {
                // Получаем шаблон яркости текущего блока
                // Get the brightness pattern of the current block
                var blockPattern = new double[charWidth, charHeight];
                for (int cy = 0; cy < charHeight; cy++)
                {
                    for (int cx = 0; cx < charWidth; cx++)
                    {
                        Color pixel = image.GetPixel(x + cx, y + cy);
                        int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                        blockPattern[cx, cy] = brightness / 255.0;
                    }
                }

                string bestMatch = asciiChars[0];
                double minDifference = double.MaxValue;

                // Находим наиболее подходящий символ из библиотеки
                // Find the most suitable character from the library
                foreach (var kvp in charPatterns)
                {
                    double difference = CalculatePatternDifference(blockPattern, kvp.Value);
                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        bestMatch = kvp.Key;
                    }
                }
                resultBuilder.Append(bestMatch);
            }
            resultBuilder.Append('\n');
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Преобразует изображение в ASCII-графику с помощью самого продвинутого алгоритма, используя расширенную библиотеку символов.
    /// Converts the image to ASCII graphics using the most advanced algorithm, utilizing an extended library of characters.
    /// </summary>
    private static string ConvertToSuperAscii(Bitmap image)
    {
        StringBuilder resultBuilder = new StringBuilder();

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                int index = (int)(brightness / 255.0 * (SuperAsciiCharacters.Length - 1));
                char charToUse = SuperAsciiCharacters[index];

                resultBuilder.Append(charToUse);
            }
            resultBuilder.Append('\n');
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Генерирует изображение в формате PNG для полноцветного символьного портрета, учитывая прозрачность пикселей.
    /// Generates a PNG image for a full-color character portrait, considering pixel transparency.
    /// </summary>
    private static Bitmap ConvertToUniversalImage(Bitmap image)
    {
        using (var font = new Font("Courier New", 12, FontStyle.Regular, GraphicsUnit.Pixel))
        using (var tempBitmap = new Bitmap(1, 1))
        using (var graphics = Graphics.FromImage(tempBitmap))
        {
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            SizeF charSize = graphics.MeasureString("W", font);

            int charWidth = (int)Math.Ceiling(charSize.Width);
            int charHeight = (int)Math.Ceiling(charSize.Height);

            int newWidth = image.Width * charWidth;
            int newHeight = image.Height * charHeight;

            var newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

            using (var newImageGraphics = Graphics.FromImage(newImage))
            {
                newImageGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);
                        // Проверяем прозрачность пикселя
                        // Check pixel transparency
                        if (pixel.A > 0)
                        {
                            int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                            int index = (int)(brightness / 255.0 * (SuperAsciiCharacters.Length - 1));

                            char charToUse = SuperAsciiCharacters[index];

                            using (var brush = new SolidBrush(pixel))
                            {
                                newImageGraphics.DrawString(charToUse.ToString(), font, brush, x * charWidth, y * charHeight);
                            }
                        }
                    }
                }
            }
            return newImage;
        }
    }

    /// <summary>
    /// Генерирует шаблон яркости для одного символа, рендеря его в bitmap.
    /// Generates a brightness pattern for a single character by rendering it to a bitmap.
    /// </summary>
    private static double[,] GenerateCharPattern(string character)
    {
        int width = 8;
        int height = 8;
        var pattern = new double[width, height];
        using (var bitmap = new Bitmap(width, height))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Black);
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            graphics.DrawString(character, new Font("Courier New", 6), Brushes.White, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    pattern[x, y] = pixel.GetBrightness();
                }
            }
        }
        return pattern;
    }

    /// <summary>
    /// Вычисляет разницу между двумя шаблонами яркости с использованием евклидовой метрики.
    /// Calculates the difference between two brightness patterns using Euclidean metric.
    /// </summary>
    private static double CalculatePatternDifference(double[,] pattern1, double[,] pattern2)
    {
        double sum = 0;
        for (int y = 0; y < pattern1.GetLength(1); y++)
        {
            for (int x = 0; x < pattern1.GetLength(0); x++)
            {
                sum += Math.Pow(pattern1[x, y] - pattern2[x, y], 2);
            }
        }
        return Math.Sqrt(sum);
    }

    /// <summary>
    /// Применяет фильтр резкости к изображению для улучшения качества.
    /// Applies a sharpening filter to the image to enhance quality.
    /// </summary>
    private static Bitmap EnhanceImageQuality(Bitmap originalImage)
    {
        int width = originalImage.Width;
        int height = originalImage.Height;
        Bitmap newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Ядро фильтра резкости
        // Sharpening filter kernel
        int[,] kernel = new int[,]
        {
            { 0, -1, 0 },
            { -1, 5, -1 },
            { 0, -1, 0 }
        };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int r = 0, g = 0, b = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        Color pixel = originalImage.GetPixel(x + kx, y + ky);
                        int weight = kernel[ky + 1, kx + 1];
                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                    }
                }

                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));

                newImage.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }

        // Копируем края изображения, которые не были обработаны
        // Copy the edges of the image that were not processed
        for (int i = 0; i < width; i++)
        {
            newImage.SetPixel(i, 0, originalImage.GetPixel(i, 0));
            newImage.SetPixel(i, height - 1, originalImage.GetPixel(i, height - 1));
        }
        for (int i = 0; i < height; i++)
        {
            newImage.SetPixel(0, i, originalImage.GetPixel(0, i));
            newImage.SetPixel(width - 1, i, originalImage.GetPixel(width - 1, i));
        }

        return newImage;
    }

    /// <summary>
    /// Вспомогательный метод для распространения ошибки при дизеринге, корректируя цвета соседних пикселей.
    /// Helper method for spreading error in dithering, adjusting colors of neighboring pixels.
    /// </summary>
    private static void SpreadError(Bitmap image, int x, int y, int error, int numerator, int denominator)
    {
        Color p = image.GetPixel(x, y);
        int r = Math.Min(255, Math.Max(0, p.R + (int)(error * (double)numerator / denominator)));
        int g = Math.Min(255, Math.Max(0, p.G + (int)(error * (double)numerator / denominator)));
        int b = Math.Min(255, Math.Max(0, p.B + (int)(error * (double)numerator / denominator)));
        image.SetPixel(x, y, Color.FromArgb(r, g, b));
    }

    /// <summary>
    /// Инвертирует цвета изображения, меняя каждый компонент цвета на обратный.
    /// Inverts the colors of the image, changing each color component to its inverse.
    /// </summary>
    private static void InvertImageColors(Bitmap image)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                image.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
            }
        }
    }

    /// <summary>
    /// Настраивает яркость и контраст изображения, применяя линейные преобразования к каждому пикселю.
    /// Adjusts the brightness and contrast of the image, applying linear transformations to each pixel.
    /// </summary>
    private static void AdjustImage(Bitmap image, int brightness, int contrast)
    {
        float b = brightness / 100.0f;
        float c = contrast / 100.0f;
        float contrastFactor = (1.0f + c);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int r = (int)((pixel.R / 255.0f * contrastFactor + b) * 255.0f);
                int g = (int)((pixel.G / 255.0f * contrastFactor + b) * 255.0f);
                int bVal = (int)((pixel.B / 255.0f * contrastFactor + b) * 255.0f);

                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                bVal = Math.Min(255, Math.Max(0, bVal));

                image.SetPixel(x, y, Color.FromArgb(r, g, bVal));
            }
        }
    }

    /// <summary>
    /// Генерирует HTML-код для цветного ASCII-изображения с использованием CSS-стилей.
    /// Generates HTML code for a color ASCII image using CSS styles.
    /// </summary>
    private static string GenerateHtml(Bitmap image, string[] asciiChars, string ditheringType)
    {
        StringBuilder htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<!DOCTYPE html>\n<html>\n<head>\n");
        htmlBuilder.Append("<title>ASCII Art</title>\n");
        htmlBuilder.Append("<style>\n");
        htmlBuilder.Append("body { background-color: #000; font-family: 'Courier New', monospace; font-size: 1px; line-height: 1; }\n");
        htmlBuilder.Append(".pixel { display: inline-block; width: 10px; height: 10px; }\n");
        htmlBuilder.Append("pre { margin: 0; line-height: 1; }\n");
        htmlBuilder.Append("</style>\n</head>\n<body>\n<pre>\n");

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                string hexColor = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}";
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                int index = (int)(brightness / 255.0 * (asciiChars.Length - 1));

                string charToUse = asciiChars[index];

                htmlBuilder.Append($"<span style=\"color:{hexColor};\">{charToUse}</span>");
            }
            htmlBuilder.Append('\n');
        }

        htmlBuilder.Append("</pre>\n</body>\n</html>");
        return htmlBuilder.ToString();
    }
}