using NetTopologySuite.Geometries; // ספרייה לעבודה עם צורות גיאומטריות (Points, Polygons, וכו')
using NetTopologySuite.Features;   // ספרייה שמוסיפה אובייקט 'Feature' (גיאומטריה + מידע נוסף)

// מחלקה ראשית שבה מתבצעת כל הלוגיקה של התוכנית
class GISBufferAnalysis
{
    // נקודת הכניסה הראשית של התוכנית (Entry Point)
    static void Main()
    {
        // יצירת "מפעל גיאומטריות" המשמש לייצור אובייקטי גיאומטריה (נקודות, קווים, פוליגונים)
        var geometryFactory = new GeometryFactory();

        // יצירת רשימה של Feature (כאילו "ישויות גאוגרפיות"), 
        // כאשר כל Feature מכיל שם בית חולים ונקודת ציון (Coordinate)
        var points = new List<Feature>
        {
            // יצירת Feature עבור "Hospital A" עם נקודת ציון (34.7818, 32.0853)
            CreateFeature("Hospital A", geometryFactory.CreatePoint(new Coordinate(34.7818, 32.0853))),
            // יצירת Feature עבור "Hospital B" עם נקודת ציון (34.8569, 32.1093)
            CreateFeature("Hospital B", geometryFactory.CreatePoint(new Coordinate(34.8569, 32.1093))),
            // יצירת Feature עבור "Hospital C" עם נקודת ציון (34.8964, 32.1495)
            CreateFeature("Hospital C", geometryFactory.CreatePoint(new Coordinate(34.8964, 32.1495))),
            // יצירת Feature עבור "Hospital D" עם נקודת ציון (34.9393, 32.0626)
            CreateFeature("Hospital D", geometryFactory.CreatePoint(new Coordinate(34.9393, 32.0626)))
        };

        // הגדרת גודל הרדיוס לבאפר: 5000 יחידות (בדרך כלל זה יכול לייצג מטרים)
        double bufferRadius = 5000; // 5 ק"מ
        // רשימה חדשה שתכיל את כל הבאפרים (פוליגונים) שניצור סביב כל נקודה
        var buffers = new List<Feature>();

        // עבור כל נקודה (Feature) ברשימת הנקודות - ניצור באפר ונשמור אותו ברשימת buffers
        foreach (var point in points)
        {
            // הפעלת הפעולה Buffer על הנקודה, כדי ליצור פוליגון סביב הרדיוס שהוגדר
            var buffer = point.Geometry.Buffer(bufferRadius);
            // הוספת ה-Feature החדש (כולל הגיאומטריה של באפר ומאפיינים מקוריים) לרשימת buffers
            buffers.Add(new Feature(buffer, point.Attributes));
        }

        // קריאה לפונקציה שמייצאת את הפוליגונים לקובץ GeoJSON בשם "buffers.geojson"
        WriteToGeoJSON("buffers.geojson", buffers);
        // הודעה למשתמש שמאשרת את שמירת הקובץ
        Console.WriteLine("Buffers have been saved to buffers.geojson");

        // קריאה לפונקציה שמאתרת חפיפות (Intersections) בין הבאפרים שהוגדרו
        var overlappingHospitals = FindOverlappingBuffers(buffers);

        // הודעה שמופיעה לפני פירוט החפיפות
        Console.WriteLine("Hospitals in overlapping areas:");

        // מעבר על הרשימה של זוגות בתי החולים החופפים
        foreach (var pair in overlappingHospitals)
        {
            // pair.Item1 ו-pair.Item2 הם שמות בתי החולים שחופפים
            Console.WriteLine($"{pair.Item1} overlaps with {pair.Item2}");
        }
    }

    /// <summary>
    /// פונקציה ליצירת Feature חדש, הכולל שם וגיאומטריה.
    /// </summary>
    /// <param name="name">שם הישות הגאוגרפית (למשל, בית חולים)</param>
    /// <param name="geometry">גיאומטריה של הישות (נקודה, פוליגון וכו')</param>
    /// <returns>Feature שמחזיק גם גיאומטריה וגם טבלת מאפיינים</returns>
    static Feature CreateFeature(string name, Geometry geometry)
    {
        // יצירת טבלת מאפיינים (attributes) חדשה
        var attributesTable = new AttributesTable
        {
            // שמירת המפתח "name" עם ערך string התואם לשם שהתקבל
            { "name", name }
        };
        // החזרת Feature שמכיל את הגיאומטריה וטבלת המאפיינים
        return new Feature(geometry, attributesTable);
    }

    /// <summary>
    /// מאתרת חפיפות (Intersections) בין באפרים ומחזירה רשימה של זוגות (Tuple)
    /// של שמות בתי חולים החופפים.
    /// </summary>
    /// <param name="buffers">רשימת Feature שמייצגת את כל הבאפרים</param>
    /// <returns>רשימה של זוגות (שם1, שם2) המסמלים חפיפה בין שני באפרים</returns>
    static List<Tuple<string, string>> FindOverlappingBuffers(List<Feature> buffers)
    {
        // רשימה שתאחסן את כל הזוגות של בתי החולים שחופפים
        var overlappingList = new List<Tuple<string, string>>();

        // שתי לולאות מקוננות לבדיקת כל צמד באפרים (בלי להשוות באפר עם עצמו פעמיים)
        for (int i = 0; i < buffers.Count; i++)
        {
            for (int j = i + 1; j < buffers.Count; j++)
            {
                // בדיקה אם הגיאומטריה של הבאפר ה-i חופפת את הגיאומטריה של הבאפר ה-j
                if (buffers[i].Geometry.Intersects(buffers[j].Geometry))
                {
                    // שליפת שמות בתי החולים מה-Attributes של כל Feature
                    string name1 = buffers[i].Attributes["name"].ToString();
                    string name2 = buffers[j].Attributes["name"].ToString();

                    // הוספת זוג (Tuple) של השמות לרשימת החפיפות
                    overlappingList.Add(new Tuple<string, string>(name1, name2));
                }
            }
        }

        // החזרת כל הזוגות (שם בית חולים1, שם בית חולים2) שנמצאו בחפיפה
        return overlappingList;
    }

    /// <summary>
    /// כותבת את הפוליגונים (או כל גיאומטריה אחרת) לתוך קובץ GeoJSON.
    /// </summary>
    /// <param name="filePath">נתיב הקובץ שאליו ייכתב המידע (למשל: buffers.geojson)</param>
    /// <param name="features">רשימת Feature שמכילה את הגיאומטריות לכתיבה</param>
    static void WriteToGeoJSON(string filePath, List<Feature> features)
    {
        // אובייקט לכתיבת גיאומטריות לפורמט GeoJSON
        var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();

        // פתיחת קובץ לכתיבה (יוצר קובץ חדש, ואם קיים - רושם עליו מחדש)
        using (var writer = File.CreateText(filePath))
        {
            // עבור כל Feature ברשימת הפיצ'רים
            foreach (var feature in features)
            {
                // המרת הגיאומטריה של ה-Feature למחרוזת בטקסט GeoJSON
                // וכתיבתה בשורה נפרדת בקובץ
                writer.WriteLine(geoJsonWriter.Write(feature.Geometry));
            }
        }
        // בסוף ה-using, ה-Stream נסגר אוטומטית, והקובץ נשמר.
    }
}
