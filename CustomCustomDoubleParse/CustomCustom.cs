using System;
using System.Globalization;
using System.Linq;

namespace CustomCustomDoubleParse
{
    class CustomCustom
    {
        const string comment = "custom custom: ";
        static double CustomCustomParse(string s)
        {
            char charPositive = CultureInfo.CurrentCulture.NumberFormat.PositiveSign[0];
            char charNegative = CultureInfo.CurrentCulture.NumberFormat.NegativeSign[0];
            // Пытается превратить строку показателя степени 10 в целое число типа short.
            bool TryParseExp(string expString, out short result)
            {
                result = 0;// Значение показателя.
                int length = expString.Length;// Длина строки показателя.
                if (length == 0) return false;// Показатель должен присутствовать.
                short expSign = 1;// Знак показателя.
                int startIndex = 0;// Индекс начального символа строки показателя в аналитическом цикле. 
                char curChar = expString[0];// Текущий символ показателя.
                // Первым символом может быть знак минус или плюс.
                // Тогда индекс начального символа увеличивается на единицу.
                if (curChar == charNegative)
                {
                    expSign = -1;
                    startIndex++;
                }
                if (curChar == charPositive)
                    startIndex++;
                // Аналитический цикл, в котором проверяется каждый символ строки показателя.
                for (int i = startIndex; i < length; i++)
                {
                    curChar = expString[i];
                    if (!char.IsDigit(curChar))
                        // Если текущий символ не является цифрой выполнение метода завершается.
                        return false;
                    try
                    {
                        checked// Проверяется на переполнение.
                        {
                            // Накполенный результат умножается на 10, сдвигаясь влево, и добавляется отступ текущего символа от нуля.
                            result = (short)(result * 10 + (curChar - '0'));
                        }
                    }
                    catch
                    {
                        return false;// При переполнении показатель выходит за границы интервала значений типа short.
                    }
                }
                result *= expSign;// Учитываем знак показателя.
                return true;
            }
            // Хранит дробную часть числа.
            ulong fractionalPart = 0;
            // Строка дробной части.
            string sFractionalPart = string.Empty;
            // Пытается превратить строку дробной части вводимого числа в целое число типа ulong.
            bool TryParseFractionalPart()
            {
                for (int i = 0; i < sFractionalPart.Length; i++)
                {
                    char curChar = sFractionalPart[i];
                    if (!char.IsDigit(curChar))
                        return false;
                    checked
                    {
                        fractionalPart = (fractionalPart * 10) + (ulong)(curChar - '0');
                    }
                }
                return true;
            }
            // Проверяем входную строку.
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s = s.Trim()))
                s = "0";
            // Определяем индекс символа отделения мантиссы от порядка ('e' или 'E'), если он существует.
            int expSymbolIndex = s.IndexOf(s.FirstOrDefault(c => c == 'e' || c == 'E'));
            // Разделяем строку на мантиссу и порядок при наличии символа разделения порядка.
            bool containsExpSymbol = expSymbolIndex > -1;
            // Определяем показатель степени 10 из строки, следующей за символом разделения.
            short exp = 0;
            // В авторском алгоритме допускается в качестве показателя степени пустая строка и строка, содержащая только знак - или знак +.
            string sExp = containsExpSymbol ? s.Substring(expSymbolIndex + 1) : string.Empty;
            if (!(string.IsNullOrEmpty(sExp) || string.IsNullOrEmpty(sExp.Trim()) || sExp == charPositive.ToString() || sExp == charNegative.ToString()) &&
                !TryParseExp(sExp, out exp))
                return Double.NaN;
            // Определяем строку мантиссы, находящейся в левой части строки перед символом разделения порядка e или E.
            string sMantissa = containsExpSymbol ? s.Substring(0, expSymbolIndex) : s;
            // Если мантисса отсутствует, то в стандарте должна возбуждаться исключительная ситуация.
            // В авторском алгоритме пустая мантисса принимает значение 1.
            // Наличие пустой мантиссы говорит о присутствии порядка, т.к. вся строка s не может быть пустой на входе в этот фрагмент кода.
            if (sMantissa.Trim().Length == 0)
                sMantissa = "1";
            // Далее анализируем мантиссу.
            // Присутствие символа десятичной токи/запятой.
            char charDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            bool containsSeparator = sMantissa.Contains(charDecimalSeparator);

            // Отделяем в строке мантиссы строку целой части от строки дробной части.
            // Индекс символа точки/запятой в строке мантиссы.
            // По умолчанию предполагается отсутствие этого символа.
            int separatorIndex = sMantissa.Length;
            if (containsSeparator)
            {
                separatorIndex = sMantissa.IndexOf(charDecimalSeparator);
                // Сохраняем строку дробной части.
                sFractionalPart = sMantissa.Substring(separatorIndex + 1);
                // В авторской версии пустая дробная часть отвечает нулю.
                if (string.IsNullOrEmpty(sFractionalPart))
                    sFractionalPart = "0";
            }
            // Строка целой части находится слева от сепаратора.
            string sIntPart = sMantissa.Substring(0, separatorIndex);
            // Формируем целую часть мантиссы.
            // Если целая часть пустая, либо не начинается со знака, то устанавливаем у нее знак +.
            if (sIntPart.Length == 0 || !(sIntPart[0] == charPositive || sIntPart[0] == charNegative)) sIntPart = string.Concat(charPositive, sIntPart);
            // При этом первым символом является знак числа.
            double sign = sIntPart[0] == charPositive ? 1 : -1;
            // Сохраняем строку модуля целой части (без знака).
            string sAbsIntPart = sIntPart.Substring(1);
            // Если целая часть отстутствует, то она должна быть нулем при наличии запятой и единицей, если есть степень и нет запятой.
            if (string.IsNullOrEmpty(sAbsIntPart)) sAbsIntPart = containsExpSymbol && !containsSeparator ? "1" : "0";
            // Если дробная часть отстутствует, то она должна быть нулем.
            if (string.IsNullOrEmpty(sFractionalPart)) sFractionalPart = "0";
            // Нормализуем строку, как если бы это было число,
            // целая часть которого равна 0, а дробная начинается с ненулевого значения.
            // Показатель степени 10 и строка дробной части могут быть изменены в процессе нормализации.
            int firstNotZeroIndex = 0;// Индекс первого символа, отличного от нуля в целой и дробной части числа.
            // Если в модуле целой части есть символы, не равные нулю.
            if (!sAbsIntPart.All(c => c == '0'))
            {
                // Находим первый ненулевой символ и определяем число символов, следующих за ним, включая сам этот символ.
                firstNotZeroIndex = sAbsIntPart.IndexOf(sAbsIntPart.First(c => c != '0'));
                // Изменяем строку дробной части, добавляя к ней слева фрагмент целой части, начиная с ненулевого символа.
                sFractionalPart = sAbsIntPart.Substring(firstNotZeroIndex) + sFractionalPart;
                // Этот сдвиг компенсируем изменением показателя.
                exp += (short)(sAbsIntPart.Length - firstNotZeroIndex);
            }
            // В целой части остается ноль.
            // Если в дробной части есть символы, не равные нулю, 
            if (!sFractionalPart.All(c => c == '0'))
            {
                // Ищем первый символ, не равный нулю.
                firstNotZeroIndex = sFractionalPart.IndexOf(sFractionalPart.First(c => c != '0'));
                // Сдвигаем дробную часть на число нулей, предшествующих не равному нулю символу.
                sFractionalPart = sFractionalPart.Substring(firstNotZeroIndex);
                // Компенсируем сдвиг изменением показателя степени.
                exp -= (short)firstNotZeroIndex;
            }
            // Теперь строка дробной части не начинается с нуля.
            // Обрезаем слишком длинную строку дробной части, учитывая ограниченное число значащих цифр. Берем 17.
            // Это позволит представить строку дробной части в виде целого числа типа ulong.
            if (sFractionalPart.Length > 17)
                sFractionalPart = sFractionalPart.Substring(0, 17);
            // Преобразуем полученную строку в целое число типа ulong.
            if (!TryParseFractionalPart())
                return Double.NaN;
            // Проверяем на переполнение.
            if (exp > 308 || exp == 309 && fractionalPart > 17976931348623157)
                return sign < 0 ? Double.NegativeInfinity : Double.PositiveInfinity;
            // Минимальное значение отличное от нуля равно double.Epsilon.
            return exp < -324 || exp == -323 && fractionalPart < 494065645841247 ? 0 : sign * fractionalPart * Math.Pow(10, -sFractionalPart.Length + exp);
        }
        static void Main()
        {
            Console.WriteLine($"Основные постоянные: Epsilon = {double.Epsilon} MinValue = {double.MinValue} MaxValue = {double.MaxValue}");
            do
            {
                Console.Write("Enter a double ");
                string aDoubleString = Console.ReadLine();
                // Standard Parse
                try
                {
                    Console.WriteLine($"standard: {double.Parse(aDoubleString)}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("standard: " + e.Message);
                }
                // Custom Parse
                try
                {
                    Console.WriteLine(comment + $"{CustomCustomParse(aDoubleString)}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(comment + e.Message);
                }
                Console.WriteLine("Strike esc to exit or any other key to restart ");
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
