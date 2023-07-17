using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnomalyDetectionWebService.Models.Utils;

using System.Text.Json.Serialization;

// Span should be list of 2 elements: [start_inclusive, end_exclusive]
using Span = System.Collections.Generic.List<long>;

//instead of namespace AnomalyAlgorithm
namespace AnomalyDetectionWebService.Models
{
    public class CorrelatedFeatures
    {
        // names of the correlated features
        public string feature1 { get; set; }
        public string feature2 { get; set; }
        public float correlation { get; set; }
        public float threshold { get; set; }
        public string typeName { get; set; }
        // you can add here your object,
        // the reason not use general object is beacuse json loader from text won't know to treat it as Line etc...
        public Line correlationObject_line { get; set; }
        public Circle correlationObject_circle { get; set; }
    
        [JsonConstructorAttribute]
        public CorrelatedFeatures() { }
    }

    // class with static method to calculate normal_model and detection of anomalies
    public class AnomalyDetection
    {
        /* The Normal model learning, and detection process use general function,
         * which you can implements by yourself and add them to CheckerMethods, ThresholdMethods dictionaries.
         * 
         * ThresholdFactory returns partial CorrelatedFeatures whose [threshold, typeName, correlationObject] was set,
         * must check each float value x is: AnomalyAlgorithim.IsRegularNum(x) [line.a, line.b, threshold etc..] else return null;
         * threshold should be excat value, without giving extra space.
         * if no correlation was found, ThresholdFactory will return null.
         * 
         * CheckerMethods returns true iff (correlation was of their responsiblities like "minimal_circle" AND a anomaly was found according to threshold)
         */
        private delegate bool AnomalousChecker(float x, float y, CorrelatedFeatures c);
        private static readonly Dictionary<string, AnomalousChecker> CheckerMethods = new Dictionary<string, AnomalousChecker>() {
            { "regression", AnomalousChecker_linear},
            { "circle", AnomalousChecker_circle},
            { "hybrid", AnomalousChecker_hybrid}

        };

        private delegate CorrelatedFeatures ThresholdFactory(float[] x, float[] y); // <float, string, object>
        // 
        private static readonly Dictionary<string, ThresholdFactory> ThresholdMethods = new Dictionary<string, ThresholdFactory>() {
            { "regression", ThresholdFactory_linear},
            { "circle", ThresholdFactory_circle},
            { "hybrid", ThresholdFactory_hybrid}

        };

        public delegate bool ShouldContinue();
        public static bool AlwaysContinue() { return true; }

        // Return if the method name is supported by the AnomalyDetection
        public static bool IsSupportedMethod(string algorithmMethod)
        {
            return CheckerMethods.ContainsKey(algorithmMethod) && ThresholdMethods.ContainsKey(algorithmMethod);
        }
        // Get normal model of specific learning-method, learning will abort if shouldContinueLearning() == false and will return null;
        public static List<CorrelatedFeatures> GetNormal(Dictionary<string, List<float>> features, string method, ShouldContinue shouldContinueLearning, bool commutative = true)
        {
            if (!ThresholdMethods.ContainsKey(method)) return new List<CorrelatedFeatures>();
            return GetNormal(features, ThresholdMethods[method], shouldContinueLearning, commutative);
        }
        // Get normal model of specific learning-method, learning will abort if shouldContinueLearning() == false and will return null;
        private static List<CorrelatedFeatures> GetNormal(Dictionary<string, List<float>> features, ThresholdFactory create, ShouldContinue shouldContinueLearning, bool commutative = true)
        {
            List<CorrelatedFeatures> result = new List<CorrelatedFeatures>();
            List<String> orderedFeatures = new List<string>();
            foreach (var s in features.Keys) orderedFeatures.Add(s);
            // for each feature as being feautre 1 [left feature]
            for (int i = 0; i < orderedFeatures.Count; i++)
            {
                // check if it's still relevant to learn[ or model has already deleted, for example]
                // check only in outer loop, but inner loop is very fast
                if (!shouldContinueLearning()) return null;

                int mostCorrelative_idx = -1;
                float mostCorrelative_pearson = 0;

                string f1 = orderedFeatures[i];
                // for each another feature 2 as being right feature, find the most correlative feature 2 to feature 1 (larger abs(pearson))
                // commutative-param: set if NOT to use upper triangle matrix method to find correlation,
                // if commutative-param and A-B correlation then B-A correlation (since correlation dependes on most abs pearson which is commutative-function)
                int j = commutative ? 0 : i + 1;
                for (; j < orderedFeatures.Count; j++)
                {
                    if (i == j) continue;
                    string f2 = orderedFeatures[j];
                    float[] x = features[f1].ToArray();
                    float[] y = features[f2].ToArray();
                    var p = MathUtil.Pearson(x, y);
                    if (Math.Abs(p) <= Math.Abs(mostCorrelative_pearson)) continue;
                    mostCorrelative_idx = j;
                    mostCorrelative_pearson = p;
                }
                if (mostCorrelative_idx == -1) continue;
                // partial setup for <float threshold, string typeName, T correlationObject_T> fields,
                CorrelatedFeatures tmp = create(features[f1].ToArray(), features[orderedFeatures[mostCorrelative_idx]].ToArray());
                // or tmp == null if no correlation or error
                if (tmp == null) continue;
                // give 'safe' space
                tmp.threshold = tmp.threshold * 1.1f;
                if (!IsRegularNum(mostCorrelative_pearson) || !IsRegularNum(tmp.threshold)) continue;
                // set the rest of fields that need to be set-up
                tmp.correlation = mostCorrelative_pearson;
                tmp.feature1 = f1;
                tmp.feature2 = orderedFeatures[mostCorrelative_idx];
                result.Add(tmp);
            }
            if (!shouldContinueLearning()) return null;
            return result;
        }

        // Get detection with default detection method=hybrid, it's default because no matter the correlation learning circle/regression/hybrid,
        // it will detect anomaly IFF there anomaly with the LEARNING method
        public static Dictionary<string, List<int>> GetDetection(Dictionary<string, List<float>> features, List<CorrelatedFeatures> normal_model)
        {
            return GetDetection(features, normal_model, AnomalousChecker_hybrid);
        }
        // Get detection with specific detection method
        private static Dictionary<string, List<int>> GetDetection(Dictionary<string, List<float>> features, List<CorrelatedFeatures> normal_model, AnomalousChecker checker)
        {
            Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
            foreach (var k in features.Keys) result.Add(k, new List<int>());
            foreach (CorrelatedFeatures n in normal_model)
            {
                string f1 = n.feature1;
                string f2 = n.feature2;
                List<float> x = features[f1];
                List<float> y = features[f2];
                for (int i = 0; i < x.Count; i++)
                {
                    if (checker(x[i], y[i], n))
                        result[f1].Add(i);
                }
            }
            return result;
        }

        //////////////////////////////////////////////////////////////

        // AnomalousChecker functions to check if there is anomaly of x,y according to correlation c, with each own responsibilty for one method
        // linear / circle / hybrid
        private static bool AnomalousChecker_linear(float x, float y, CorrelatedFeatures c)
        {
            Line ln = c.correlationObject_line;
            if (ln != null)
                return MathUtil.Dev(new Point() { x = x, y = y }, ln) > c.threshold;
            return false;
        }
        private static bool AnomalousChecker_circle(float x, float y, CorrelatedFeatures c)
        {
            Circle circle = c.correlationObject_circle;
            if (circle == null) return false;
            return MinimalCircle.dist(new Point() { x = x, y = y }, circle.center) > c.threshold;
        }
        private static bool AnomalousChecker_hybrid(float x, float y, CorrelatedFeatures c)
        {
            return AnomalousChecker_linear(x, y, c) || AnomalousChecker_circle(x, y, c);
        }

        //////////////////////////////////////////////////////////////

        // ThresholdFactory functions to create partail CorrelatedFeaturs  according to x,y , with each own responsibilty for one method
        // linear / circle / hybrid
        // also check that every float IsRegularNum 

        private static bool IsRegularNum(double num)
        {
            return double.IsFinite(num);
        }
        private static CorrelatedFeatures ThresholdFactory_linear(float[] x, float[] y)
        {
            if (Math.Abs(MathUtil.Pearson(x, y)) < 0.9) return null;
            var tmp = MathUtil.Reg(x, y);
            if (!IsRegularNum(tmp.a) || !IsRegularNum(tmp.b)) return null;
            double max = 0;
            for (int i = 0; i < x.Length; i++)
            {
                Point p = new Point() { x = x[i], y = y[i] };
                max = Math.Max(max, Math.Abs(MathUtil.Dev(p, tmp)));
            }
            return new CorrelatedFeatures { threshold = (float)max, typeName = "Line Regression", correlationObject_line = tmp };
        }
        private static CorrelatedFeatures ThresholdFactory_circle(float[] x, float[] y)
        {
            if (Math.Abs(MathUtil.Pearson(x, y)) < 0.5) return null;
            var tmp = MathUtil.findMinCircle(x, y);
            if (!IsRegularNum(tmp.center.x) || !IsRegularNum(tmp.center.y) || !IsRegularNum(tmp.radius)) return null;
            return new CorrelatedFeatures { threshold =  tmp.radius, typeName = "Minimal Circle", correlationObject_circle = tmp };
        }
        private static CorrelatedFeatures ThresholdFactory_hybrid(float[] x, float[] y)
        {
            // only if no linear regression was found, try to find minimal circle correlation
            return ThresholdFactory_linear(x, y) ?? ThresholdFactory_circle(x, y);
        }

        //////////////////////////////////////////////////////////////

        // Return dictionary of list of span[span=list of 2 elements: start, end(exclusive)],
        // meaning make the anomalies timesteps reports to ranges of anomalies reports
        public static Dictionary<string, List<Span>> ToSpanDictionary(Dictionary<string, List<int>> detection)
        {
            var spanDictionary = new Dictionary<string, List<Span>>();
            // for each feature
            foreach (string f in detection.Keys)
            {
                var f_span = new List<Span>();
                long range_start = -100;
                long last_timestep = -100;
                // for each time step
                foreach (var timeStep in detection[f])
                {
                    // if this is start of new range and no prev range to be added
                    if (range_start == -100)
                    {
                        range_start = timeStep;
                        last_timestep = timeStep;
                        continue;
                    }
                    // if this isn't start of new range, but it continuation of the current range
                    if (last_timestep + 1 == timeStep)
                    {
                        last_timestep = timeStep;
                        continue;
                    }
                    // if this isn't continuation then treat as is new range, and also add the prev range
                    // last_timestep + 1  since it's exclusive end
                    f_span.Add(new Span() { range_start, last_timestep + 1 });
                    last_timestep = timeStep;
                    range_start = timeStep;
                }
                // if we ended the loop within range and the range wasn't added to th span list, then add it
                if (range_start != -100)
                {
                    // last_timestep + 1  since it's exclusive end
                    f_span.Add(new Span() { range_start, last_timestep + 1 });
                }
                spanDictionary.Add(f, f_span);
            }
            return spanDictionary;
        }

        // get dictionary between each feature that had anomaly reports to short string that describes why
        public static Dictionary<string, string> GetReportTypes(List<CorrelatedFeatures> model, Dictionary<string, List<int>> detection)
        {
            Dictionary<string, string> reportTypes = new Dictionary<string, string>();
            foreach (var c in model)
                if (detection == null || (detection.ContainsKey(c.feature1) && detection[c.feature1].Count != 0))
                    // example: "featureA" : "Line Regression with FeatureB"
                    reportTypes.TryAdd(c.feature1, c.typeName + " with " + c.feature2);
            return reportTypes;
        }
    }
}
