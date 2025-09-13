# Конвертер изображений в ASCII-арт и обратно / Image-to-ASCII Art and Text-to-Image Converter

---

## Аннотация / Abstract

**Русский**:  
*AsciiArtGenerator* — это приложение на C#, которое преобразует цифровые изображения в текстовую ASCII-графику и текстовые файлы в изображения. Программа поддерживает оттенки серого, цветной режим, дизеринг (Floyd-Steinberg, Bayer), настройку яркости/контраста, фильтр резкости и экспорт в HTML или PNG. Этот документ представляет собой научную статью, подробно описывающую алгоритмы, их математические основы и реализацию, с акцентом на обучение начинающих программистов.

**English**:  
*AsciiArtGenerator* is a C# application that converts digital images into ASCII art and text files into images. It supports grayscale mapping, color rendering, dithering (Floyd-Steinberg, Bayer), brightness/contrast adjustments, sharpening filter, and export to HTML or PNG. This document serves as a scientific paper, detailing the algorithms, their mathematical foundations, and implementation, with a focus on educating novice programmers.

---

## Введение / Introduction

**Русский**:  
ASCII-арт — это форма визуального представления изображений с помощью текстовых символов, возникшая в 1960-х годах из-за ограничений ранних компьютеров, таких как телетайпы и матричные принтеры. С развитием цифровой обработки изображений появились автоматизированные инструменты для создания ASCII-арта, а современные реализации используют символы Unicode для повышения детализации. *AsciiArtGenerator* предлагает гибкий инструмент для преобразования изображений в текст и текста в изображения, включая дизеринг, цвет и Unicode-символы. Проект служит утилитой и учебным пособием для изучения обработки изображений и алгоритмов.

**English**:  
ASCII art is a visual representation of images using text characters, originating in the 1960s due to hardware limitations like teletypes and line printers. With digital image processing, automated ASCII art tools emerged, and modern implementations leverage Unicode for enhanced detail. *AsciiArtGenerator* provides a versatile tool for converting images to text and text to images, supporting dithering, color, and Unicode symbols. It serves as both a utility and an educational resource for learning image processing and algorithms.

Для дополнительной информации / For more information: [ASCII Art - Wikipedia](https://en.wikipedia.org/wiki/ASCII_art).

---

## Методы и алгоритмы / Methods and Algorithms

**Русский**:  
Здесь описаны ключевые алгоритмы преобразования изображений в ASCII-арт и текста в изображения, их математические основы и реализация в коде.

**English**:  
This section describes the core algorithms for image-to-ASCII and text-to-image conversion, their mathematical foundations, and code implementation.

### Базовое преобразование в оттенки серого / Basic Grayscale Conversion

**Русский**:  
Базовый метод преобразует изображение в ASCII, уменьшая его размер и сопоставляя яркость пикселей с символами. Для каждого пикселя вычисляется яркость по формуле:  
```
Яркость = 0.299 * R + 0.587 * G + 0.114 * B
```  
где R, G, B — значения красного, зеленого и синего (0–255). Эта формула учитывает восприятие яркости человеческим глазом, где зеленый имеет наибольший вес. Яркость нормализуется (0–1) и сопоставляется с индексом в палитре символов, отсортированной от плотных (например, `#`) к легким (например, ` `). Метод прост, но теряет детали в градиентах.  
Для изучения: [Image to ASCII - The Coding Train](https://thecodingtrain.com/challenges/166-image-to-ascii/).

**English**:  
The basic method converts an image to ASCII by downsampling it and mapping pixel brightness to characters. Brightness is calculated as:  
```
Brightness = 0.299 * R + 0.587 * G + 0.114 * B
```  
where R, G, B are red, green, blue values (0–255). This formula reflects human brightness perception, with green weighted highest. Brightness is normalized (0–1) and mapped to an index in a character palette, sorted from dense (e.g., `#`) to light (e.g., ` `). This method is simple but loses detail in gradients.  
For learning: [Image to ASCII - The Coding Train](https://thecodingtrain.com/challenges/166-image-to-ascii/).

### Цветной режим / Color Support

**Русский**:  
Цветной ASCII-арт сохраняет RGB-цвета пикселей, отображая их через цвет символов. В консоли это ограниченно (ANSI-коды), но лучший результат достигается при экспорте в HTML, где каждый символ оборачивается в `<span>` с цветом в формате `#RRGGBB`. Это улучшает визуальную точность, но требует поддержки цветного отображения.

**English**:  
Color ASCII art preserves pixel RGB colors, rendering characters with matching foreground colors. In the console, this is limited (ANSI codes), but better results are achieved with HTML export, where each character is wrapped in a `<span>` with a `#RRGGBB` color. This enhances visual fidelity but requires color-capable viewers.

### Алгоритмы дизеринга / Dithering Algorithms

Дизеринг устраняет полосы от квантования, добавляя контролируемый шум.  
Dithering eliminates banding from quantization by adding controlled noise.

#### Дизеринг Флойд-Стейнберга / Floyd-Steinberg Dithering

**Русский**:  
Алгоритм Флойд-Стейнберга (1976) сканирует изображение слева направо, сверху вниз. Для каждого пикселя:  
1. Квантуем яркость (например, до 0 или 255 в бинарном режиме).  
2. Вычисляем ошибку: `ошибка = старая_яркость - новая_яркость`.  
3. Распределяем ошибку на соседние пиксели:  
   - Справа: `7/16 * ошибка`  
   - Слева снизу: `3/16 * ошибка`  
   - Снизу: `5/16 * ошибка`  
   - Справа снизу: `1/16 * ошибка`  
Это сохраняет тональные переходы. Подробности: [Floyd–Steinberg dithering - Wikipedia](https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering).

**English**:  
Floyd-Steinberg dithering (1976) scans the image left-to-right, top-to-bottom. For each pixel:  
1. Quantize brightness (e.g., to 0 or 255 in binary mode).  
2. Compute error: `error = old_brightness - new_brightness`.  
3. Distribute error to neighbors:  
   - Right: `7/16 * error`  
   - Below-left: `3/16 * error`  
   - Below: `5/16 * error`  
   - Below-right: `1/16 * error`  
This preserves tonal transitions. Details: [Floyd–Steinberg dithering - Wikipedia](https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering).

#### Дизеринг Байера / Bayer Dithering

**Русский**:  
Дизеринг Байера использует матрицу порогов 4x4:  
```
 0  8  2 10
12  4 14  6
 3 11  1  9
15  7 13  5
```  
Матрица масштабируется (делится на 16) и повторяется по изображению. Для пикселя (x, y) порог `t = матрица[x % 4, y % 4] / 16`. Если яркость > t, выбирается светлый символ, иначе — темный. Это создает узор точек, уменьшающий артефакты. Матрица формируется рекурсивно для размеров 2^n. Подробности: [Ordered dithering - Wikipedia](https://en.wikipedia.org/wiki/Ordered_dithering).

**English**:  
Bayer dithering uses a 4x4 threshold matrix:  
```
 0  8  2 10
12  4 14  6
 3 11  1  9
15  7 13  5
```  
The matrix is scaled (divided by 16) and tiled across the image. For pixel (x, y), threshold `t = matrix[x % 4, y % 4] / 16`. If brightness > t, a light character is chosen; else, dark. This creates a dotted pattern, reducing artifacts. The matrix is recursively constructed for 2^n sizes. Details: [Ordered dithering - Wikipedia](https://en.wikipedia.org/wiki/Ordered_dithering).

### Продвинутый режим / Advanced Pattern-Matching

**Русский**:  
Продвинутый режим делит изображение на блоки (8x8 пикселей) и сравнивает их яркостные узоры с шаблонами символов. Шаблоны создаются путем рендеринга символов в bitmap (с помощью GDI+ в шрифте Courier New) и вычисления их яркости. Лучший символ выбирается по минимальной разнице:  
```
разница = sqrt(сумма((яркость_блока - яркость_символа)^2))
```  
Это повышает детализацию для структурированных изображений. Изучите: [Pattern Matching in Computer Vision](https://en.wikipedia.org/wiki/Template_matching).

**English**:  
The advanced mode divides the image into 8x8 pixel blocks and matches their brightness patterns to character templates. Templates are generated by rendering characters to bitmaps (using GDI+ with Courier New font) and computing brightness. The best character minimizes:  
```
difference = sqrt(sum((block_brightness - char_brightness)^2))
```  
This enhances detail for structured images. Study: [Pattern Matching in Computer Vision](https://en.wikipedia.org/wiki/Template_matching).

### Супер-режим с Unicode / Super Mode with Unicode

**Русский**:  
Супер-режим использует расширенную палитру Unicode-символов (например, █, ▒), отсортированных по визуальной плотности. Яркость пикселя отображается на индекс символа, что дает более плавные градиенты. Подробности о Unicode: [Unicode - Wikipedia](https://en.wikipedia.org/wiki/Unicode).

**English**:  
Super mode uses an extended Unicode character palette (e.g., █, ▒), sorted by visual density. Pixel brightness maps to a character index, providing smoother gradients. Details on Unicode: [Unicode - Wikipedia](https://en.wikipedia.org/wiki/Unicode).

### Настройка изображения / Image Adjustments

**Русский**:  
Яркость и контраст настраиваются для каждого пикселя:  
```
цвет' = (цвет / 255 * (1 + контраст) + яркость) * 255
```  
Значения ограничиваются [0, 255]. Инверсия: `255 - цвет`. Фильтр резкости использует ядро свертки для повышения четкости краев.

**English**:  
Brightness and contrast adjust each pixel:  
```
color' = (color / 255 * (1 + contrast) + brightness) * 255
```  
Values are clamped to [0, 255]. Inversion: `255 - color`. The sharpening filter uses a convolution kernel to enhance edge clarity.

### Преобразование текста в изображение / Text-to-Image Conversion

**Русский**:  
Метод преобразует текстовый файл в PNG-изображение, рендеря текст в шрифте Courier New на белом фоне с черным цветом. Размер изображения определяется длиной строк и высотой шрифта.

**English**:  
The method converts a text file to a PNG image, rendering text in Courier New font on a white background with black color. Image size is determined by line length and font height.

### Полноцветный символьный портрет / Full-Color Character Portrait

**Русский**:  
Режим создает PNG-изображение, где каждый пиксель заменяется символом из Unicode-палитры, окрашенным в цвет исходного пикселя. Учитывается прозрачность пикселей.

**English**:  
This mode creates a PNG image where each pixel is replaced by a Unicode character, colored to match the original pixel. Pixel transparency is considered.

---

## Реализация / Implementation

**Русский**:  
Проект использует библиотеку System.Drawing для обработки изображений и текста. Код организован в классе `AsciiArtGenerator`, где каждый метод выполняет отдельную задачу, включая обработку аргументов, преобразование изображений и текста, и применение фильтров.

**English**:  
The project uses the System.Drawing library for image and text processing. Code is organized in the `AsciiArtGenerator` class, with methods handling specific tasks, including argument parsing, image and text conversion, and filter application.

---

## Требования / Requirements

**Русский**:  
- .NET 6.0 или выше.  
- Visual Studio, VS Code с C# расширением или Rider.  
- Поддерживаемые форматы: JPG, PNG, BMP, TIFF (для изображений); TXT (для текста).  

**English**:  
- .NET 6.0 or higher.  
- Visual Studio, VS Code with C# extension, or Rider.  
- Supported formats: JPG, PNG, BMP, TIFF (for images); TXT (for text).  

---

## Установка / Installation

**Русский**:  
1. Клонируйте репозиторий:  
   ```
   git clone https://github.com/SavelyZhukov007/ascii-art-generator.git
   ```  
2. Скомпилируйте и запустите:  
   ```
   dotnet run
   ```

**English**:  
1. Clone the repository:  
   ```
   git clone https://github.com/SavelyZhukov007/ascii-art-generator.git
   ```  
2. Build and run:  
   ```
   dotnet run
   ```

---

## Использование / Usage

**Русский**:  
Программа работает в двух режимах: интерактивном (без аргументов) или через командную строку.  

**Для изображений**:  
- `-s <число>`: масштаб (по умолчанию 4).  
- `-o <путь>`: путь для сохранения результата.  
- `-p <строка>`: кастомная палитра символов (например, `-p "#*o. "`).  
- `-c`: включить цветной режим.  
- `-d <тип>`: дизеринг (floyd-steinberg, bayer, advanced, super).  
- `-b <число>`: яркость (-100 до 100).  
- `-k <число>`: контраст (-100 до 100).  
- `-i`: инвертировать цвета.  
- `-q`: применить фильтр резкости.  
- `-h`: экспорт в HTML.  
- `-u`: создать полноцветный символьный портрет (PNG).  

**Для текстовых файлов**:  
- `-j`: преобразовать текст в изображение (PNG).  
- `-o <путь>`: путь для сохранения результата.  

Пример для изображения:  
```
dotnet run "photo.jpg" -s 4 -d super -h
```

Пример для текста:  
```
dotnet run "input.txt" -j -o "output.png"
```

**English**:  
The program operates in two modes: interactive (no arguments) or via command line.  

**For images**:  
- `-s <number>`: scale (default 4).  
- `-o <path>`: path to save the result.  
- `-p <string>`: custom character palette (e.g., `-p "#*o. "`).  
- `-c`: enable color mode.  
- `-d <type>`: dithering (floyd-steinberg, bayer, advanced, super).  
- `-b <number>`: brightness (-100 to 100).  
- `-k <number>`: contrast (-100 to 100).  
- `-i`: invert colors.  
- `-q`: apply sharpening filter.  
- `-h`: export to HTML.  
- `-u`: create full-color character portrait (PNG).  

**For text files**:  
- `-j`: convert text to image (PNG).  
- `-o <path>`: path to save the result.  

Example for image:  
```
dotnet run "photo.jpg" -s 4 -d super -h
```

Example for text:  
```
dotnet run "input.txt" -j -o "output.png"
```

---

## Изучение кода для начинающих / Studying the Code for Beginners

**Русский**:  
Этот раздел подробно объясняет код, помогая новичкам разобраться в алгоритмах и C#. Читайте комментарии в исходном коде для контекста.

**English**:  
This section explains the code in detail, helping beginners understand algorithms and C#. Read source code comments for context.

### Точка входа: метод `Main` / Entry Point: `Main` Method

**Русский**:  
Метод `Main` — начало программы. Он проверяет аргументы командной строки. Без аргументов запускается `StartInteractiveMode()`, запрашивающий путь к файлу и параметры. Изучите, как обрабатываются аргументы с помощью `args`.  
Код:  
```csharp
public static void Main(string[] args)
{
    Console.OutputEncoding = Encoding.UTF8;
    if (args.Length > 0)
    {
        ProcessArgs(args);
    }
    else
    {
        StartInteractiveMode();
    }
}
```  
Для понимания консольных приложений: [Console Applications in .NET](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-console).

**English**:  
The `Main` method is the program’s entry point. It checks command-line arguments. Without args, it calls `StartInteractiveMode()`, prompting for file path and parameters. Study how `args` handles input.  
Code:  
```csharp
public static void Main(string[] args)
{
    Console.OutputEncoding = Encoding.UTF8;
    if (args.Length > 0)
    {
        ProcessArgs(args);
    }
    else
    {
        StartInteractiveMode();
    }
}
```  
For console apps: [Console Applications in .NET](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-console).

### Интерактивный режим: `StartInteractiveMode` / Interactive Mode: `StartInteractiveMode`

**Русский**:  
Метод запрашивает путь к файлу и определяет его тип (текст или изображение), затем предлагает параметры в зависимости от типа файла. Использует цикл для проверки существования файла и корректности ввода.  
Алгоритм: Простой интерфейс CLI с динамическими опциями. Попробуйте добавить поддержку других форматов файлов (например, PDF).

**English**:  
This method prompts for a file path, determines its type (text or image), and offers parameters based on the file type. It uses a loop to validate file existence and input.  
Algorithm: Simple CLI interface with dynamic options. Try adding support for other file formats (e.g., PDF).

### Обработка параметров: `ProcessArgs` / Parameter Parsing: `ProcessArgs`

**Русский**:  
Метод разбирает флаги (`-s`, `-d`, `-j` и т.д.) в цикле с `switch`, определяет тип файла и вызывает соответствующую функцию (`ProcessImage` или `ProcessText`). Обратите внимание на обработку исключений (например, файл не найден).  
Алгоритм: Парсинг аргументов — стандарт для CLI-утилит. Попробуйте добавить новый флаг, например, для изменения шрифта в текстовом рендеринге.

**English**:  
This method parses flags (`-s`, `-d`, `-j`, etc.) in a loop with `switch`, determines the file type, and calls the appropriate function (`ProcessImage` or `ProcessText`). Note exception handling (e.g., file not found).  
Algorithm: Argument parsing, common in CLI tools. Try adding a new flag, e.g., for changing font in text rendering.

### Базовое преобразование: `ConvertToAscii` / Core Conversion: `ConvertToAscii`

**Русский**:  
Самый простой метод преобразования изображения в ASCII. Перебирает пиксели, вычисляет яркость и сопоставляет с символом.  
```csharp
int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
int index = (int)(brightness / 255.0 * (asciiChars.Length - 1));
```  
Разберите: Формула яркости учитывает восприятие глазом. Попробуйте изменить веса (например, больше для красного).  
Ресурс: [Turn any image into ASCII art! (Python)](https://www.youtube.com/watch?v=v_raWlX7tZY).

**English**:  
The simplest method for image-to-ASCII conversion. Loops over pixels, computes brightness, and maps to a character.  
```csharp
int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
int index = (int)(brightness / 255.0 * (asciiChars.Length - 1));
```  
Dissect: Brightness formula reflects human perception. Try tweaking weights (e.g., more for red).  
Resource: [Turn any image into ASCII art! (Python)](https://www.youtube.com/watch?v=v_raWlX7tZY).

### Цветной ASCII: `ConvertToColorAscii` / Color ASCII: `ConvertToColorAscii`

**Русский**:  
Создает цветной ASCII-арт, добавляя HTML-теги `<span>` с цветами пикселей. Используется для HTML-вывода, так как консольный цвет ограничен.  
Попробуйте добавить поддержку ANSI-кодов для консоли.

**English**:  
Creates color ASCII art by adding HTML `<span>` tags with pixel colors. Used for HTML output since console color support is limited.  
Try adding ANSI code support for console output.

### Дизеринг Флойд-Стейнберга: `ConvertToAsciiWithFloydSteinbergDithering` / Floyd-Steinberg Dithering: `ConvertToAsciiWithFloydSteinbergDithering`

**Русский**:  
Клонирует изображение, квантует пиксели (0 или 255), распределяет ошибку через `SpreadError`. Коэффициенты (7/16 и т.д.) оптимизированы для шахматного узора.  
Добавьте `Console.WriteLine` для отображения ошибок.  
Ресурс: [Floyd-Steinberg Python example (Reddit)](https://www.reddit.com/r/Python/comments/saovu3/made_a_program_to_turn_images_into_ascii_art/).

**English**:  
Clones the image, quantizes pixels (0 or 255), distributes error via `SpreadError`. Coefficients (7/16, etc.) optimize for checkerboard patterns.  
Add `Console.WriteLine` to visualize errors.  
Resource: [Floyd-Steinberg Python example (Reddit)](https://www.reddit.com/r/Python/comments/saovu3/made_a_program_to_turn_images_into_ascii_art/).

### Дизеринг Байера: `ConvertToAsciiWithBayerDithering` / Bayer Dithering: `ConvertToAsciiWithBayerDithering`

**Русский**:  
Применяет матрицу 4x4, сравнивает яркость с порогом. Бинарное отображение (первый или последний символ). Попробуйте создать матрицу 8x8 рекурсивно.  
Математика: Пороги создают "синий шум", уменьшающий артефакты.

**English**:  
Applies a 4x4 matrix, compares brightness to threshold. Binary mapping (first or last char). Try generating an 8x8 matrix recursively.  
Math: Thresholds create blue noise, reducing artifacts.

### Продвинутый режим: `ConvertToAdvancedAscii` / Advanced Mode: `ConvertToAdvancedAscii`

**Русский**:  
Создает шаблоны символов через GDI+, сравнивает с блоками изображения.  
`GenerateCharPattern`: рендерит символ в 8x8 bitmap.  
`CalculatePatternDifference`: вычисляет среднеквадратичную разницу.  
Похоже на сопоставление шаблонов в компьютерном зрении. Изучите: [Pattern Matching in Computer Vision](https://en.wikipedia.org/wiki/Template_matching).

**English**:  
Generates char patterns via GDI+, matches to image blocks.  
`GenerateCharPattern`: renders char to 8x8 bitmap.  
`CalculatePatternDifference`: computes mean squared difference.  
Resembles template matching in computer vision. Study: [Pattern Matching in Computer Vision](https://en.wikipedia.org/wiki/Template_matching).

### Супер-режим: `ConvertToSuperAscii` / Super Mode: `ConvertToSuperAscii`

**Русский**:  
Использует Unicode-палитру из `GetSuperAsciiCharacters`. Плотность символов (например, █ vs. .) определяет градиенты. Тестируйте в UTF-8 терминалах.

**English**:  
Uses Unicode palette from `GetSuperAsciiCharacters`. Character density (e.g., █ vs. .) defines gradients. Test in UTF-8 terminals.

### Полноцветный портрет: `ConvertToUniversalImage` / Full-Color Portrait: `ConvertToUniversalImage`

**Русский**:  
Создает PNG, заменяя пиксели цветными Unicode-символами. Учитывает прозрачность, используя шрифт Courier New. Попробуйте изменить размер шрифта для детализации.

**English**:  
Creates a PNG by replacing pixels with colored Unicode characters. Considers transparency, using Courier New font. Try changing font size for detail.

### Преобразование текста в изображение: `ConvertTextToImage` / Text-to-Image Conversion: `ConvertTextToImage`

**Русский**:  
Рендерит текст из файла в PNG, используя белый фон и черный шрифт Courier New. Размер изображения вычисляется по длине строк и высоте шрифта.  
Попробуйте добавить поддержку цветного текста.

**English**:  
Renders text from a file to PNG, using a white background and black Courier New font. Image size is computed from line length and font height.  
Try adding support for colored text.

### Настройки изображения: `AdjustImage`, `InvertImageColors`, `EnhanceImageQuality` / Image Adjustments: `AdjustImage`, `InvertImageColors`, `EnhanceImageQuality`

**Русский**:  
`AdjustImage` изменяет яркость/контраст, `InvertImageColors` инвертирует цвета, `EnhanceImageQuality` применяет фильтр резкости с ядром свертки. Похоже на линейную коррекцию гаммы. Попробуйте добавить нелинейную гамма-коррекцию.

**English**:  
`AdjustImage` tweaks brightness/contrast, `InvertImageColors` flips colors, `EnhanceImageQuality` applies a sharpening filter with a convolution kernel. Similar to linear gamma correction. Try adding nonlinear gamma correction.

### HTML-вывод: `GenerateHtml` / HTML Export: `GenerateHtml`

**Русский**:  
Создает `<pre>` с цветными `<span>` для цветного ASCII-арта. Изучите CSS для настройки шрифта или фона.

**English**:  
Builds `<pre>` with colored `<span>`s for color ASCII art. Study CSS for font or background tweaks.

---

## Лицензия / License

MIT License — используйте свободно / Use freely.

---

## Полезные ресурсы / Useful Resources

- [Image-to-ASCII art converter AI tutorial](https://www.youtube.com/watch?v=ngabsn2i9L0)  
- [How to make ASCII Art in Adobe Photoshop](https://www.youtube.com/watch?v=iC0lSh4FM2g)  
- [Floyd-Steinberg Python example (Reddit)](https://www.reddit.com/r/Python/comments/saovu3/made_a_program_to_turn_images_into_ascii_art/)  
- [Console Applications in .NET](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-console)  
- [Pattern Matching in Computer Vision](https://en.wikipedia.org/wiki/Template_matching)  
- [Unicode - Wikipedia](https://en.wikipedia.org/wiki/Unicode)
