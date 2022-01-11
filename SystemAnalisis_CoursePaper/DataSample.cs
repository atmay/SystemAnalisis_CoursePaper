namespace SystemAnalisis_CoursePaper
{
    static class DataSample
    {
        public static int size = 40;

        public static double[] x1 = {
           -0.89226, -2.964, 2.407427, -0.95317, 0.09662, -0.9001, -1.75405, -0.52284, 1.279751, 2.882271,
           -1.53892, 0.192879, -0.41338, -1.16492, -0.93506, -0.42939, 0.504345, -1.98881, 1.907867, -0.66521,
           -0.75723, -0.95764, -0.12167, 0.441017, 0.572774, 0.159065, -0.24437, 1.192335, 2.307802, -2.43077,
           -0.12154, 1.607998, 2.863991, 0.448929, -0.90951, 0.895402, -1.64199, -2.81261, 0.799087, -1.47432
        };

        public static double[] x2 = {
            -2.82594, 2.348727, 4.867938, 4.551945, 4.362961, 0.19983, 4.975569, 4.613684, 5.37971, 1.029973,
            4.457407, 0.355628, 2.877735, 4.467752, 3.868594, 1.047429, 1.173919, 0.850284, 1.081194, 2.024751,
            5.270139, 3.042793, 0.732759, 3.980216, 5.442886, 3.222946, 2.513169, 5.291304, 4.181383, 0.68943,
            2.307911, 2.917626, 0.182288, 1.404388, 5.452539, 5.866652, 0.397018, 0.342916, 5.599449, 2.578642,
        };

        public static double[] x3 = {
            2.437804, 2.288619, 0.206897, 2.863508, 1.472163, 1.184338, -0.4663, 1.045543, -0.52373, 2.478775,
            0.305943, 1.852033, -0.05244, -0.28462, 2.528682, -0.94162, 2.859801, 2.247144, 0.06252, -0.54769,
            1.399765, 0.569844, 0.264706, 0.563958, -0.41052, 2.717488, -0.80085, 2.556176, 0.770374, 0.507027,
            1.664577, 0.121235, 1.374925, 2.571356, -0.94326, 2.750845, 1.091988, 0.137372, 1.870341, 0.050122,
        };

        public static double[] y = {
            -32.2243, -3.18629, -4.63969, -2.31684, -4.00867, 2.806639, -13.1085, -6.48999, -8.79858, 12.36579,
            -8.17719, 5.664358, -5.54466, -9.92461, -2.72403, -4.58737, 6.253611, 1.513798, 3.859126, -4.82765,
            -7.38277, -5.08878, 1.932457, -2.99832, -10.1103, 1.848969, -5.21345, 0.498303, 0.16664, -4.23068,
            0.2336, -1.07273, 10.82174, 6.616666, -15.2128, -2.21036, 0.729021, -3.85939, -2.65273, -6.40084
        };

        public static string baseFormula = "y = a0 + a1*x1 + a2*x2 + a3*x3";
        public static string extendedFormula = "y = a0 + a1*x1 + a2*x2 + a3*x3 + a4*{0}";
        public static string[] variantsNames =
        {
            "x1*x1",
            "x1*x2",
            "x1*x3",
            "x2*x2",
            "x2*x3",
            "x3*x3"
        };

        public static double[,] GetSXMatrix(int start, int end)
        {
            var len = end - start;
            var matrix = new double[len, 4];

            for (int i = start; i < end; i++)
            {
                var k = i - start;
                matrix[k, 0] = 1;
                matrix[k, 1] = x1[i];
                matrix[k, 2] = x2[i];
                matrix[k, 3] = x3[i];
            }

            return matrix;
        }

        public static double[,] GetCorrectedSXMatrix(int start, int end, int variant)
        {
            var len = end - start;
            var matrix = new double[len, 5];

            for (int i = start; i < end; i++)
            {
                var k = i - start;
                matrix[k, 0] = 1;
                matrix[k, 1] = x1[i];
                matrix[k, 2] = x2[i];
                matrix[k, 3] = x3[i];

                switch (variant)
                {
                    case 0:
                        matrix[k, 4] = x1[i] * x1[i];
                        break;
                    case 1:
                        matrix[k, 4] = x1[i] * x2[i];
                        break;
                    case 2:
                        matrix[k, 4] = x1[i] * x3[i];
                        break;
                    case 3:
                        matrix[k, 4] = x2[i] * x2[i];
                        break;
                    case 4:
                        matrix[k, 4] = x2[i] * x3[i];
                        break;
                    case 5:
                        matrix[k, 4] = x3[i] * x3[i];
                        break;
                }
                
            }

            return matrix;
        }
        public static double[,] GetYMatrix(int start, int end)
        {
            var len = end - start;
            var matrix = new double[len, 1];

            for (int i = start; i < end; i++)
            {
                var k = i - start;
                matrix[k, 0] = y[i];
            }
            return matrix;
        }
    }
}
