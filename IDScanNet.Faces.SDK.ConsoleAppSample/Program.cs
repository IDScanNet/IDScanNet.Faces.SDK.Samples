using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IDScanNet.Faces.SDK.ConsoleAppSample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("IDScan.net Faces SDK ConsoleAppSample");
            Console.WriteLine();

            // path to license file
            var licenseFilePath = Path.Combine(Path.Combine(AppContext.BaseDirectory,"idscannet.facesdk.license"));

            var settings = new Settings
            {
                // this is a temporary dummy license
                License = File.ReadAllBytes(licenseFilePath),
                RecognitionPreset = RecognitionPreset.Balanced
            };

            // create and initialize face service
            using var service = new FaceService(settings);

            var testDataDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "test_data"));

            var image1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image1.jpg"));
            var image1_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image1_1.jpg"));
            
            var image2 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image2.jpg"));
            var image2_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image2_1.jpg"));
            
            var image3 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image3.jpg"));
            var image3_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image3_1.jpg"));
            
            var image4 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image4.jpg"));
            var image4_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image4_1.jpg"));
            
            var image5 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image5.jpg"));
            var image5_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image5_1.jpg"));
            
            var image6 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image6.jpg"));
            var image6_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image6_1.jpg"));
            
            var image7 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image7.jpg"));
            var image7_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image7_1.jpg"));
            
            var image8 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image8.jpg"));
            var image8_1 = File.ReadAllBytes(Path.Combine(testDataDirectory,"image8_1.jpg"));

            
            // detect faces from images
            using var sample1 = await service.DetectSingleAsync(image1);
            using var sample1_1 = await service.DetectSingleAsync(image1_1);
            
            using var sample2 = await service.DetectSingleAsync(image2);
            using var sample2_1 = await service.DetectSingleAsync(image2_1);
            
            using var sample3 = await service.DetectSingleAsync(image3);
            using var sample3_1 = await service.DetectSingleAsync(image3_1);
            
            using var sample4 = await service.DetectSingleAsync(image4);
            using var sample4_1 = await service.DetectSingleAsync(image4_1);
            
            using var sample5 = await service.DetectSingleAsync(image5);
            using var sample5_1 = await service.DetectSingleAsync(image5_1);
            
            using var sample6 = await service.DetectSingleAsync(image6);
            using var sample6_1 = await service.DetectSingleAsync(image6_1);
            
            using var sample7 = await service.DetectSingleAsync(image7);
            using var sample7_1 = await service.DetectSingleAsync(image7_1);
            
            using var sample8 = await service.DetectSingleAsync(image8);
            using var sample8_1 = await service.DetectSingleAsync(image8_1);

            
            // compute face templates
            var personGuid1 = Guid.NewGuid();
            using var template1 = await service.ComputeTemplateAsync(sample1);
            template1.Tag = "1";
            template1.PersonGuid = personGuid1;
            using var template1_1 = await service.ComputeTemplateAsync(sample1_1);
            template1_1.Tag = "1_1";
            template1_1.PersonGuid = personGuid1;
            
            var personGuid2 = Guid.NewGuid();
            using var template2 = await service.ComputeTemplateAsync(sample2);
            template2.Tag = "2";
            template2.PersonGuid = personGuid2;
            using var template2_1 = await service.ComputeTemplateAsync(sample2_1);
            template2_1.Tag = "2_1";
            template2_1.PersonGuid = personGuid2;
            
            var personGuid3 = Guid.NewGuid();
            using var template3 = await service.ComputeTemplateAsync(sample3);
            template3.Tag = "3";
            template3.PersonGuid = personGuid3;
            using var template3_1 = await service.ComputeTemplateAsync(sample3_1);
            template3_1.Tag = "3_1";
            template3_1.PersonGuid = personGuid3;
            
            var personGuid4 = Guid.NewGuid();
            using var template4 = await service.ComputeTemplateAsync(sample4);
            template4.Tag = "4";
            template4.PersonGuid = personGuid4;
            using var template4_1 = await service.ComputeTemplateAsync(sample4_1);
            template4_1.Tag = "4_1";
            template4_1.PersonGuid = personGuid4;
            
            var personGuid5 = Guid.NewGuid();
            using var template5 = await service.ComputeTemplateAsync(sample5);
            template5.Tag = "5";
            template5.PersonGuid = personGuid5;
            using var template5_1 = await service.ComputeTemplateAsync(sample5_1);
            template5_1.Tag = "5_1";
            template5_1.PersonGuid = personGuid5;
            
            var personGuid6 = Guid.NewGuid();
            using var template6 = await service.ComputeTemplateAsync(sample6);
            template6.Tag = "6";
            template6.PersonGuid = personGuid6;
            using var template6_1 = await service.ComputeTemplateAsync(sample6_1);
            template6_1.Tag = "6_1";
            template6_1.PersonGuid = personGuid6;
            
            var personGuid7 = Guid.NewGuid();
            using var template7 = await service.ComputeTemplateAsync(sample7);
            template7.Tag = "7";
            template7.PersonGuid = personGuid7;
            using var template7_1 = await service.ComputeTemplateAsync(sample7_1);
            template7_1.Tag = "7_1";
            template7_1.PersonGuid = personGuid7;
            
            var personGuid8 = Guid.NewGuid();
            using var template8 = await service.ComputeTemplateAsync(sample8);
            template8.Tag = "8";
            template8.PersonGuid = personGuid8;
            using var template8_1 = await service.ComputeTemplateAsync(sample8_1);
            template8_1.Tag = "8_1";
            template8_1.PersonGuid = personGuid8;



            // compare two templates of the same person
            var matchResult1 = await service.MatchAsync(template1, template1_1);
            Console.WriteLine($"person1 match result: {matchResult1.Score * 100f} %  ({matchResult1})");

            var matchResult2 = await service.MatchAsync(template2, template2_1);
            Console.WriteLine($"person2 match result: {matchResult2.Score * 100f} %  ({matchResult2})");
            
            var matchResult3 = await service.MatchAsync(template3, template3_1);
            Console.WriteLine($"person3 match result: {matchResult3.Score * 100f} %  ({matchResult3})");
            
            var matchResult4 = await service.MatchAsync(template4, template4_1);
            Console.WriteLine($"person4 match result: {matchResult4.Score * 100f} %  ({matchResult4})");
            
            var matchResult5 = await service.MatchAsync(template5, template5_1);
            Console.WriteLine($"person5 match result: {matchResult5.Score * 100f} %  ({matchResult5})");
            
            var matchResult6 = await service.MatchAsync(template6, template6_1);
            Console.WriteLine($"person6 match result: {matchResult6.Score * 100f} %  ({matchResult6})");
            
            var matchResult7 = await service.MatchAsync(template7, template7_1);
            Console.WriteLine($"person7 match result: {matchResult7.Score * 100f} %  ({matchResult7})");
            
            var matchResult8 = await service.MatchAsync(template8, template8_1);
            Console.WriteLine($"person8 match result: {matchResult8.Score * 100f} %  ({matchResult8})");

            Console.WriteLine();
            
            var templates = new List<ITemplate>
            {
                template1,
                template1_1,
                template2,
                template2_1,
                template3,
                template3_1,
                template4,
                template4_1,
                template5,
                template5_1,
                template6,
                template6_1,
                template7,
                template7_1,
                template8,
                template8_1,
            };
            
            // compare all templates with all
            foreach (var templateA in templates)
            {
                foreach (var templateB in templates)
                {
                    var matchResult = await service.MatchAsync(templateA, templateB);
                    Console.WriteLine($"{templateA.Tag} and {templateB.Tag} match result: {matchResult.Score * 100f} %  ({matchResult})");
                }
            }

            var list = templates.Where(t => t.Tag != template1.Tag).ToList();
            var searchResult = await service.SearchAsync(template1, list);
            Console.WriteLine($"Search: {searchResult.ConfidencePercent} %, matched template: {searchResult.MatchedTemplate.Tag}");
            
            // index variant
            using var index = await service.CreateIndexAsync(list);
            
            var searchResult1 = await service.SearchAsync(template1, index);
            Console.WriteLine($"Search: {searchResult1.ConfidencePercent} %, matched template: {searchResult.MatchedTemplate.Tag}");
            
            
        }
    }
}
