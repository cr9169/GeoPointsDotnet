using NetTopologySuite.Geometries;
using NetTopologySuite.Features;

class GISBufferAnalysis
{
    static void Main()
    {
       
        var geometryFactory = new GeometryFactory();

     
        var points = new List<Feature>
        {
            CreateFeature("Hospital A", geometryFactory.CreatePoint(new Coordinate(34.7818, 32.0853))),
            CreateFeature("Hospital B", geometryFactory.CreatePoint(new Coordinate(34.8569, 32.1093))),
            CreateFeature("Hospital C", geometryFactory.CreatePoint(new Coordinate(34.8964, 32.1495))),
            CreateFeature("Hospital D", geometryFactory.CreatePoint(new Coordinate(34.9393, 32.0626)))
        };

  
        double bufferRadius = 5000; // 5 ק"מ ברדיוס
        var buffers = new List<Feature>();

        foreach (var point in points)
        {
            var buffer = point.Geometry.Buffer(bufferRadius);
            buffers.Add(new Feature(buffer, point.Attributes));
        }

      
        WriteToGeoJSON("buffers.geojson", buffers);
        Console.WriteLine("Buffers have been saved to buffers.geojson");

     
        var overlappingHospitals = FindOverlappingBuffers(buffers);

        Console.WriteLine("Hospitals in overlapping areas:");
        // הצגת בתי חולים שנמצאים בחפיפה
        foreach (var pair in overlappingHospitals)
        {
            // שימוש ב-Tuple להצגת זוג שמות בתי החולים בצורה קריאה
            Console.WriteLine($"{pair.Item1} overlaps with {pair.Item2}");
        }
    }

  
    static Feature CreateFeature(string name, Geometry geometry)
    {
        var attributesTable = new AttributesTable
        {
            { "name", name }
        };
        return new Feature(geometry, attributesTable);
    }


    static List<Tuple<string, string>> FindOverlappingBuffers(List<Feature> buffers)
    {
        var overlappingList = new List<Tuple<string, string>>();

        for (int i = 0; i < buffers.Count; i++)
        {
            for (int j = i + 1; j < buffers.Count; j++)
            {
                // בדיקה אם יש חפיפה בין באפרים
                if (buffers[i].Geometry.Intersects(buffers[j].Geometry))
                {
                    string name1 = buffers[i].Attributes["name"].ToString();
                    string name2 = buffers[j].Attributes["name"].ToString();
                     // שמירת שמות בתי החולים כ-Tuple: פשוט, קריא, ומייצג זוג ערכים בלבד
                    overlappingList.Add(new Tuple<string, string>(name1, name2));
                }
            }
        }

        return overlappingList;
    }


    static void WriteToGeoJSON(string filePath, List<Feature> features)
    {
        var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();

        using (var writer = File.CreateText(filePath))
        {
            foreach (var feature in features)
            {
                writer.WriteLine(geoJsonWriter.Write(feature.Geometry));
            }
        }
    }
}