using NetTopologySuite.Geometries; // ספרייה שמספקת כלים לעבודה עם צורות גיאומטריות
using NetTopologySuite.Features;   // ספרייה לעבודה עם 'Feature' (גיאומטריה + מידע טבלאי)

class GISBufferAnalysis
{
    static void Main()
    {
        var geometryFactory = new GeometryFactory(); // מפעל ליצירת צורות גיאומטריות

        // רשימה של נקודות (כ-Features), כל אחת עם שם ובנקודות ציון
        var points = new List<Feature>
        {
            CreateFeature("Hospital A", geometryFactory.CreatePoint(new Coordinate(34.7818, 32.0853))),
            CreateFeature("Hospital B", geometryFactory.CreatePoint(new Coordinate(34.8569, 32.1093))),
            CreateFeature("Hospital C", geometryFactory.CreatePoint(new Coordinate(34.8964, 32.1495))),
            CreateFeature("Hospital D", geometryFactory.CreatePoint(new Coordinate(34.9393, 32.0626)))
        };

        double bufferRadius = 5000; // גודל הבאפר (5 ק"מ)
        var buffers = new List<Feature>();

        // יצירת באפר (צורת פוליגון) לכל נקודה ושמירתו ברשימה
        foreach (var point in points)
        {
            var buffer = point.Geometry.Buffer(bufferRadius);
            buffers.Add(new Feature(buffer, point.Attributes));
        }

        // כתיבת הפוליגונים ל-GeoJSON
        WriteToGeoJSON("buffers.geojson", buffers);
        Console.WriteLine("Buffers have been saved to buffers.geojson");

        // איתור חפיפות בין הפוליגונים
        var overlappingHospitals = FindOverlappingBuffers(buffers);

        Console.WriteLine("Hospitals in overlapping areas:");

        // הדפסת זוגות בתי חולים שחופפים
        foreach (var pair in overlappingHospitals)
        {
            // שימוש ב-Tuple: בזוג הזה Item1 ו-Item2 הם שמות בתי החולים
            Console.WriteLine($"{pair.Item1} overlaps with {pair.Item2}");
        }
    }

    // פונקציה ליצירת Feature עם שם וגיאומטריה
    static Feature CreateFeature(string name, Geometry geometry)
    {
        var attributesTable = new AttributesTable
        {
            { "name", name }
        };
        return new Feature(geometry, attributesTable);
    }

    // בודקת אילו באפרים חופפים, ומחזירה זוגות של שמות בתי חולים
    static List<Tuple<string, string>> FindOverlappingBuffers(List<Feature> buffers)
    {
        var overlappingList = new List<Tuple<string, string>>();

        for (int i = 0; i < buffers.Count; i++)
        {
            for (int j = i + 1; j < buffers.Count; j++)
            {
                // אם יש חפיפה בין שני באפרים
                if (buffers[i].Geometry.Intersects(buffers[j].Geometry))
                {
                    string name1 = buffers[i].Attributes["name"].ToString();
                    string name2 = buffers[j].Attributes["name"].ToString();
                    overlappingList.Add(new Tuple<string, string>(name1, name2));
                }
            }
        }

        return overlappingList;
    }

    // פונקציה לכתיבת הפוליגונים שהתקבלו לקובץ GeoJSON
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
