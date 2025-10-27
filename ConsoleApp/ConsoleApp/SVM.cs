using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVM
{
    class SupportVectorMachine
    {
        //it's the sample of Support Vector Machine.  The functionality is not so completed yet. It is only for classifying 2 classes.
        //reference:
        //[0]:http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.43.4376&rep=rep1&type=pdf
        //[1]:https://ocw.mit.edu/courses/sloan-school-of-management/15-097-prediction-machine-learning-and-statistics-spring-2012/lecture-notes/MIT15_097S12_lec12.pdf
        //[2]:https://ocw.mit.edu/courses/electrical-engineering-and-computer-science/6-034-artificial-intelligence-fall-2010/tutorials/MIT6_034F10_tutor05.pdf
        //I use Gaussian Elimation method to solve the  matrix, though this is not the best method to solve linear equations, but it's easier to understand
        //referance:
        //[3]:https://martin-thoma.com/solving-linear-equations-with-gaussian-elimination/
        // I found that Gaussian Elimination is not so good at dealing large Matrix (more than 20*20), and there is some problem on underdetermined linear system 
        //, so I survey to the simplified SMO
        //reference:
        //[4]:http://cs229.stanford.edu/materials/smo.pdf

        private double[][] INPUTVALUES;
        //2 dimensional array, can cantain several elements each rows,for instance, IRIS_data={sepal_length,sepal_width,petal_length,petal_width};
        private int[] LABELS;
        //shall specify +1 -1 corresponding to INPUTVALUES, it's as the output of f(x),to build SVM.
        /*       
        By Using Lagrange Multiplier Method,amd we simplify KKT conditions to get following Constraints:
        C0: w_vector=sumation(alpha_i*y_i*x_i)
        C1.sumation(alpha_i*y_i)=0
        C2. positive gutter function:y_i*(Trans_w_vector*x_i) sumation(alpha_i*y_i*kernel(x_i,x))+b=+1
        C3. negative gutter function: sumation(alpha_i*y_i*kernel(x_i,x))+b=-1
        C4. alpha_i>=0
        */
        public double[] W_vector;
        /*
         * the normal vector  we shall evaluate to geneate classification function :
         * y=Trans(W_vector)(x_vector)+b
         * 
         * 
        */
        public double b;
        /*
         * the coefficient b is in this function:     y=Trans(W_vector)(x_vector)+b
         * 
        */
        public SupportVectorMachine(double[][] TrainingData, int[] Traininglabels, KernelType tp)
        {
            if (TrainingData.Length > 0 && TrainingData.Length == Traininglabels.Length)
            {
                this.INPUTVALUES = TrainingData;
                this.LABELS = Traininglabels;
               
               
                SimplifiedSMO SSMO = new SimplifiedSMO(INPUTVALUES, LABELS,1e1, tp);
                for (int i = 0; i <SSMO.ALPHAs.Length; i++)
                {
                    Console.WriteLine("alpha[{0}] is:{1}", i, SSMO.ALPHAs[i]);
                }
                W_vector = Wvector_Evaluate(INPUTVALUES,LABELS,SSMO.ALPHAs);
                b = SSMO.b;
            }
            else
            {
                if (TrainingData.Length <= 0)
                {
                    Console.WriteLine("Length of Traning Data is wrong");
                }
                if (Traininglabels.Length != TrainingData.Length)
                {
                    Console.WriteLine("data length of labels and inputvalues are not the same");
                }
            }
        }
        public SupportVectorMachine(double[] Previous_W_vector, double Previous_b)
        {
            this.W_vector = Previous_W_vector;
            this.b = Previous_b;
        }
        public int[] Predict(double[][] input_values)
        {
            int[] RESULTS = new int[input_values.Length];
            for (int i = 0; i < input_values.Length; i++)
            {
                double res = 0;
                for (int j = 0; j < input_values[i].Length; j++)
                {
                    res += W_vector[j] * input_values[i][j];
                }
                res += b;
                if (res >= 1)
                {
                    RESULTS[i] = 1;
                }
                else if (res <= -1)
                {
                    RESULTS[i] = -1;
                }
                else
                {
                    RESULTS[i] = 0;
                }
            }
            return RESULTS;
        } 
        private double[] Wvector_Evaluate(double[][] input_values,int[]labels, double[] alphas)
        {
            double[] W = new double[input_values[0].Length];
            for (int i = 0; i < input_values.Length; i++)
            {
                for (int j = 0; j < input_values[i].Length; j++)
                {
                    W[j] += alphas[i] * labels[i] * input_values[i][j];
                    //by KKT C0: w_vector=sumation(alpha_i*y_i*x_i)
                }
            }
            return W;
        }
    }
}
