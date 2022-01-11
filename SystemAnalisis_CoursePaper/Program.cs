using System;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SystemAnalisis_CoursePaper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isAdequate = MakeAdequateModel();

            if (isAdequate == true)
            {
                Console.WriteLine($"Модель адекватна без корректировки!");
                Console.ReadLine();
                return;
            }

            // КОРРЕКТИРОВКА МОДЕЛИ (при необходимости)

            for ( int variant=0; variant<6; variant++)
            {
                var formula = string.Format(DataSample.extendedFormula, DataSample.variantsNames[variant]);
                Console.WriteLine($"Try find model for:\n\t{formula}\n");

                // определяем начало и конец обучающей и контрольной выборок
                var (start1, end1, start2, end2) = FindBestRange();

                // получаем обучающие матрицы для x и y
                Matrix<double> SXTrainingCorrected = DenseMatrix.OfArray(DataSample.GetCorrectedSXMatrix(start1, end1, variant));
                Matrix<double> YTraining = DenseMatrix.OfArray(DataSample.GetYMatrix(start1, end1));
                Console.WriteLine("Матрица y:\n" + YTraining.ToString());

                // определяем параметры модели (значения коэффициентов) по формуле метода наименьших квадратов
                Matrix<double> SXT = SXTrainingCorrected.Transpose();
                Matrix<double> A = (SXT * SXTrainingCorrected).Inverse() * SXT * YTraining;

                Console.WriteLine("Матрица модели:\n" + A.ToString());

                // получаем контрольные матрицы для x и y
                Matrix<double> SXControl = DenseMatrix.OfArray(DataSample.GetCorrectedSXMatrix(start2, end2, variant));
                Matrix<double> YControl = DenseMatrix.OfArray(DataSample.GetYMatrix(start2, end2));

                // вычисляем вектор ошибок на основе контрольной выборки
                Matrix<double> YVector = SXControl * A;
                Matrix<double> E = YControl - YVector;
                
                Console.WriteLine("Матрица ошибок:\n"+E.ToString());

                // получаем J - оценку качества модели
                double J = GetQuality(DataSample.y, YVector, start2, end2);

                Console.WriteLine($"J = {J}");

                // в три этапа проверяем адекватность полученной модели

                // этап 1. отсутствие тренда (проверка с использованием критерия знаков)
                // 1.1 вычисляем последовательность 0 и 1 для временного ряда
                int[] zeroOneSeq = ZeroOneSeq(E, end2 - start2);

                // вычисляем количество серий и длину наибольшей серии
                var (sequenceQuantity, sequenceMaxLen) = GetSequenceStat(zeroOneSeq);
                Console.WriteLine($"Количество серий = {sequenceQuantity} Максимальная длина = {sequenceMaxLen}");

                // определяем наличие либо отсутствие тренда
                bool notTrend = IsTrend(DataSample.size / 2, sequenceMaxLen, sequenceQuantity);
                Console.WriteLine($"Не тренд = {notTrend}");

                // этап 2. отсутствие автокорреляции
                bool notAutocorllation = IsAutocorellation(DataSample.size / 2, E);
                Console.WriteLine($"Нет автокорреляции = {notAutocorllation}");

                // этап 3. нормальное распределение
                bool normalSpread = IsNormalSpread(DataSample.size / 2, E);
                Console.WriteLine($"Нормальное распределение = {normalSpread}");

                bool isCorrectedAdequate = notTrend && notAutocorllation && normalSpread;
                Console.WriteLine($"Откорректированная модель адекватна = {isCorrectedAdequate}");

                Console.WriteLine($"Расчеты для модели:\t{formula}");
                Console.WriteLine("\n\n\n");
            }
            Console.ReadLine();
        }


        static bool MakeAdequateModel()
        {
            // определяем начало и конец обучающей и контрольной выборок
            var (start1, end1, start2, end2) = FindBestRange();
            Console.WriteLine($"Начало и окончание обучающей выборки: {start1}, {end1}\n Начало и окончание контрольной выборки: {start2}, {end2}\n");

            // получаем обучающие матрицы для x и y
            Matrix<double> SXTraining = DenseMatrix.OfArray(DataSample.GetSXMatrix(start1, end1));
            Matrix<double> YTraining = DenseMatrix.OfArray(DataSample.GetYMatrix(start1, end1));

            Console.WriteLine("Матрица x:\n" + SXTraining.ToString());
            Console.WriteLine("Матрица y:\n" + YTraining.ToString());

            // определяем параметры модели (значения коэффициентов) по формуле метода наименьших квадратов
            Matrix<double> SXT = SXTraining.Transpose();
            Matrix<double> A = (SXT * SXTraining).Inverse() * SXT * YTraining;

            Console.WriteLine("Матрица модели:\n" + A.ToString());

            // получаем контрольные матрицы для x и y
            Matrix<double> SXControl = DenseMatrix.OfArray(DataSample.GetSXMatrix(start2, end2));
            Matrix<double> YControl = DenseMatrix.OfArray(DataSample.GetYMatrix(start2, end2));

            // вычисляем вектор ошибок на основе контрольной выборки
            Matrix<double> YVector = SXControl * A;
            Matrix<double> E = YControl - YVector;

            Console.WriteLine("Матрица ошибок:\n"+E.ToString());

            // получаем J - оценку качества модели
            double J = GetQuality(DataSample.y, E, start2, end2);

            Console.WriteLine($"J = {J}");

            // в три этапа проверяем адекватность полученной модели

            // этап 1. отсутствие тренда (проверка с использованием критерия знаков)
            // 1.1 вычисляем последовательность 0 и 1 для временного ряда
            int[] zeroOneSeq = ZeroOneSeq(E, end2 - start2);

            // вычисляем количество серий и длину наибольшей серии
            var (sequenceQuantity, sequenceMaxLen) = GetSequenceStat(zeroOneSeq);

            // определяем наличие либо отсутствие тренда
            bool notTrend = IsTrend(DataSample.size / 2, sequenceMaxLen, sequenceQuantity);

            // этап 2. отсутствие автокорреляции
            bool notAutocorllation = IsAutocorellation(DataSample.size / 2, E);

            // этап 3. нормальное распределение
            bool normalSpread = IsNormalSpread(DataSample.size / 2, E);

            bool isAdequate = notTrend && notAutocorllation && normalSpread;

            return isAdequate;
        }

        // метод для разбиения выборки на контрольную и обучающую (возвращает точки начала и конца каждой из частей)
        static (int, int, int, int) FindBestRange()
        {
            double avg = GetAverageY(DataSample.y);
            double deviation1 = GetDeviationY(DataSample.y, avg, 0, DataSample.size / 2);
            double deviation2 = GetDeviationY(DataSample.y, avg, DataSample.size / 2, DataSample.size);
            if (deviation1 > deviation2)
            {
                return (
                    0, DataSample.size / 2,
                    DataSample.size / 2, DataSample.size);
            }

            return (
                DataSample.size / 2, DataSample.size,
                0, DataSample.size / 2);
        }

        // вспомогательный метод для определения среднего значения у для исходной выборки
        static double GetAverageY(double[] y)
        {
            int len = y.Length;
            double sum = 0;
            for (int i = 0; i < len; i++)
                sum += y[i];
            return sum / len;
        }

        // вспомогательный метод для определения отклонения у для заданной части выборки
        static double GetDeviationY(double[] y, double avg, int start, int end)
        {
            double deviation = 0;
            for (int i = start; i < end; i++)
            {
                Console.WriteLine($"{y[i] - avg}");
                deviation += Math.Abs(y[i] - avg);
            }
            return deviation;
        }

        // метод для определения качества модели
        static double GetQuality(double[] y, Matrix<double> yVector, int start, int end)
        {
            double quadratic = 0;
            for (int i = start; i < end; i++)
            {
                var k = i - start;
                double yAbs = y[i] - yVector[k, 0];
                quadratic += yAbs * yAbs;
            }
            int M = end - start;
            double result =  quadratic / M;
            return result;
        }

        // вспомогательный метод для генерации последовательности 0 и 1 (тренд)
        static int[] ZeroOneSeq(Matrix<double> E, int len)
        {
            int[] seq = new int [len];

            for (int i = 1; i < len; i++)
            {
                if ((E[i, 0] - E[i-1, 0]) > 0)
                {
                    seq[i] = 1;
                }
                else
                {
                    seq[1] = 0;
                }
            }
            return seq;
        }

        // вспомогательный метод для вычисления количества серий и максимальной продолжительности серии (тренд)
        static (int, int) GetSequenceStat(int[] seq)
        {
            int seqLen = seq.Length;
            
            int qty = 1;
            int maxSize = 1;
            int curSize = 1;
            
            for (int i = 1; i < seqLen; i++)
            {
                if (seq[i] != seq[i - 1])
                {
                    qty++;
                    curSize = 0;
                }

                curSize += 1;
                if (curSize > maxSize)
                    maxSize = curSize;
            }

            return (qty, maxSize);
        }

        // метод для определения наличия тренда, возвращает true/false
        static bool IsTrend(int sampleLen, int maxSeqSize, int seqQty)
        {
            bool isGoodSeqQty = seqQty > Math.Abs(Math.Floor(1 / 3 * (2 * sampleLen - 1) 
                - 1.96 * Math.Sqrt((16 * sampleLen - 29) / 90)));
            bool isGoodSeqSize = (maxSeqSize <= 5);

            return isGoodSeqQty && isGoodSeqSize;
        }

        // метод для вычисления критерия Дарбина-Уотсона (автокорреляция)
        static double GetDarbinWatson(int sampleLen, Matrix<double> E) 
        {
            double quadDiff = 0;
            double quadSum = 0;

            for (int i = 1; i < sampleLen; i++)
            {
                quadDiff += Math.Pow((E[i, 0] - E[i - 1, 0]), 2);
            }

            for (int i = 0; i < sampleLen; i++)
            {
                quadSum += Math.Pow((E[i, 0]), 2);
            }

            return quadDiff / quadSum;
        }

        // метод для определения автокорреляции 
        static bool IsAutocorellation(int sampleLen, Matrix<double> E)
        {
            double d = GetDarbinWatson(sampleLen, E);

            double dMin = 1.68;
            double dMax = 4 - dMin;

            return dMin <= d && d <= dMax;
        }

        static double GetEPow(Matrix<double> E, int power, int sampleLen)
        {
            double result = 0;
            for (int i = 0; i < sampleLen; i++)
            {
                result += Math.Pow((E[i, 0]), power);
            }
            return result;
        }

        // метод для определения нормальности распределения
        static bool IsNormalSpread(int sampleLen, Matrix<double> E)
        {
            // получаем необходимые данные для вычисления формулы
            double quadE = GetEPow(E, 2, sampleLen);
            double s = Math.Sqrt(quadE / (sampleLen - 1));

            double tripleE = GetEPow(E, 3, sampleLen);
            double quatrE = GetEPow(E, 4, sampleLen);

            double assymetryErr = Math.Sqrt((6 * (sampleLen - 2)) / ((sampleLen + 1) * (sampleLen + 3)));
            double excessErr = Math.Sqrt((24 * sampleLen * (sampleLen - 2) * (sampleLen - 3)) / 
                               (Math.Pow((sampleLen - 1), 2) * (sampleLen + 3) * (sampleLen + 5)));

            double assymetry = tripleE / (sampleLen * Math.Pow(s, 3));
            double excess = quatrE / (sampleLen * Math.Pow(s, 4)) - 3;
            Console.WriteLine($"assymetryErr = {assymetryErr}");
            Console.WriteLine($"excessErr = {excessErr}");
            Console.WriteLine($"assymetry = {assymetry}");
            Console.WriteLine($"excess = {excess}");

            if ((Math.Abs(assymetry) >= 2 * s) | (Math.Abs(excess + (6 / sampleLen + 1)) >= 2 * s))
                return false;

            if ((Math.Abs(assymetry) < 1.5 * s) & (Math.Abs(excess + (6 / sampleLen + 1)) < 1.5 * s))
                return true;

            return false;
        }
    }
}
