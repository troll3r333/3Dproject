using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
using System.Linq;

namespace _3Dproject
{
    public class _3Dproject
    {
        [CommandMethod("PaintAcreage")]
        public void SelectObject()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Yêu cầu người dùng chọn một đối tượng
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

                        // Duyệt qua các mặt và hiển thị thông tin diện tích
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in faces)
                        {
                            // Lấy diện tích của từng mặt
                            double area = face.GetArea();
                            ed.WriteMessage($"\nMặt ID: {face.GetHashCode()}, Diện tích: {area}");
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
}

