using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
using System.Linq;

public class BrepSelection
{
    [CommandMethod("SelectObject")]
    public void SelectObject()
    {

        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        Database db = doc.Database;

        PromptEntityOptions peo = new PromptEntityOptions("\nChọn một đối tượng 3D: ");
        peo.SetRejectMessage("\nĐối tượng không phải là một khối 3D.");
        peo.AddAllowedClass(typeof(Solid3d), true);
        PromptEntityResult per = ed.GetEntity(peo);

        // Kiểm tra kết quả lựa chọn
        if (per.Status != PromptStatus.OK)
        {
            ed.WriteMessage("\nKhông có đối tượng 3D nào được chọn.");
            return;
        }
        
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;

            if (ent is Solid3d solid3d)
            {

                // Tạo một BRep từ đối tượng Solid3D
                using (Brep brep = new Brep(solid3d))
                {
                    ed.WriteMessage("\nĐã truy xuất BRep từ đối tượng 3D.");

                    // Lấy các thông tin về các mặt của BRep
                    BrepFaceCollection faces = brep.Faces;
                    ed.WriteMessage($"\nSố lượng mặt: {faces.Count()}");

                    // Duyệt qua các mặt và hiển thị thông tin
                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in faces)
                    {
                        ed.WriteMessage($"\nMặt ID: {face.GetHashCode()}");
                    }

                    // Lấy thông tin về các cạnh của BRep
                    BrepEdgeCollection edges = brep.Edges;
                    ed.WriteMessage($"\nSố lượng cạnh: {edges.Count()}");

                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Edge edge in edges)
                    {
                        Curve3d curve3d = edge.Curve;
                        if (curve3d != null)
                        {
                            Point3d startPoint = curve3d.StartPoint;
                            Point3d endPoint = curve3d.EndPoint;
                            double tolerance = 1e-6;

                            double startParam = curve3d.GetParameterOf(startPoint, new Tolerance(1e-3, 1e-3));
                            double endParam = curve3d.GetParameterOf(endPoint, new Tolerance(1e-3, 1e-3));

                            double length = curve3d.GetLength(startParam, endParam, tolerance);
                            ed.WriteMessage($"\nChiều dài cạnh: {length}");
                        }
                        else
                        {
                            ed.WriteMessage("\nKhông thể lấy Curve3d từ cạnh.");
                        }

                    }
                }
            }
            else
            {
                ed.WriteMessage("\nĐối tượng không phải là một khối 3D hợp lệ.");
            }
            tr.Commit();
        }
    }
}
