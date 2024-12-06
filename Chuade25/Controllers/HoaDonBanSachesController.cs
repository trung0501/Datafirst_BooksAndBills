using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Chuade25.Models;

namespace Chuade25.Controllers
{
    public class HoaDonBanSachesController : Controller
    {
        private QLSachEntities db = new QLSachEntities();
        //private readonly QLSachEntities _context;
        //public HoaDonBanSachesController(QLSachEntities context)
        //{
        //    _context = context;
        //}
        // GET: HoaDonBanSaches
        public ActionResult Index()
        {
            var hoaDonBanSaches = db.HoaDonBanSaches.Include(h => h.Sach);
            return View(hoaDonBanSaches.ToList());
        }
        // Sách bán ra nhiều nhất
        public ActionResult SachMax()
        {
            // Bước 1: Tính tổng số lượng sách bán ra ứng với từng mã sách
            var ds = db.HoaDonBanSaches.Include("Sach").GroupBy(x => x.MaSach)
                .Select(g => new
                {
                    MaSach = g.Key,
                    TenSach = g.FirstOrDefault().Sach.TenSach,
                    TheLoai = g.FirstOrDefault().Sach.TheLoai,
                    TacGia = g.FirstOrDefault().Sach.TacGia,
                    DonGia = g.FirstOrDefault().Sach.DonGia,
                    SoLuongTon = g.FirstOrDefault().Sach.SoLuongTon,
                    Tongban = g.Sum(x => x.SoLuong)
                }).ToList();
            // Bước 2: Số lượng bán nhiều nhất là bao nhiêu?
            int max = 0;
            if (ds.Count > 0) max = ds.Max(x => x.Tongban);
            // Bước 3: Đưa ra những quyển sách có số lượng bán =Max
           
            var dsmax = ds.Where(x => x.Tongban == max)
                 .Select(x => new  Sachmax
                 {
                     MaSach = x.MaSach,
                     TenSach = x.TenSach,
                     TacGia = x.TacGia,
                     TheLoai = x.TheLoai,
                     DonGia = x.DonGia,
                     SoLuongTon = x.SoLuongTon,  
                     TongSLBan=x.Tongban
                 }).ToList();
            return View(dsmax);
        }
        // Đưa ra những hoá đơn có thành tiền bán ra nhiều nhất
        public ActionResult Timmax()
        {
            //Bươc 1: lấy được thông tin của hoá đơn có thành tiền lớn nhất
            HoaDonBanSach hd = db.HoaDonBanSaches.OrderByDescending(x => x.SoLuong * x.Sach.DonGia).FirstOrDefault();
            //Bước 2: Tìm Max- hoá đơn có thành tiền max
            decimal max = 0;
            if (hd != null) max = hd.SoLuong * hd.Sach.DonGia;
            // Gửi biến Max lên view
            ViewBag.TTMax = max;
            // Đưa ra danh sách các hoá đơn có thành tiền =max
            var dsmax = db.HoaDonBanSaches.Where(x => x.SoLuong * x.Sach.DonGia == max).ToList();
            return View(dsmax);
        }
        // TÌm kiếm
        [HttpPost]
        public ActionResult Timkiem(string txtTenKH)
        {
            var ds = db.HoaDonBanSaches.Include("Sach").Where(x => x.KhachHang.Contains(txtTenKH)).ToList();
            return View("Index", ds);
        }
        // GET: HoaDonBanSaches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDonBanSach hoaDonBanSach = db.HoaDonBanSaches.Find(id);
            if (hoaDonBanSach == null)
            {
                return HttpNotFound();
            }
            return View(hoaDonBanSach);
        }

        // GET: HoaDonBanSaches/Create
        public ActionResult Create()
        {
            ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach");
            return View();
        }

        // POST: HoaDonBanSaches/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHD,MaSach,KhachHang,NgayBan,SoLuong")] HoaDonBanSach hoaDonBanSach)
        {
            if (ModelState.IsValid)
            {
                ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach");
                if (!hoaDonBanSach.KTSoLuong(db))
                {
                    ModelState.AddModelError("SoLuong", "Trường số lượng phải nhỏ hơn số lượng tồn");
                    return View(hoaDonBanSach);
                }              
                    db.HoaDonBanSaches.Add(hoaDonBanSach);
                    db.SaveChanges();
                    return RedirectToAction("Index");                
            }
            ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach", hoaDonBanSach.MaSach);
            return View(hoaDonBanSach);
        }

        // GET: HoaDonBanSaches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDonBanSach hoaDonBanSach = db.HoaDonBanSaches.Find(id);
            if (hoaDonBanSach == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach", hoaDonBanSach.MaSach);
            return View(hoaDonBanSach);
        }

        // POST: HoaDonBanSaches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHD,MaSach,KhachHang,NgayBan,SoLuong")] HoaDonBanSach hoaDonBanSach)
        {
            if (ModelState.IsValid)
            {
                ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach");
                if (!hoaDonBanSach.KTSoLuong(db))
                {
                    ModelState.AddModelError("SoLuong", "Trường số lượng phải nhỏ hơn số lượng tồn");
                    return View(hoaDonBanSach);
                }
                db.Entry(hoaDonBanSach).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaSach = new SelectList(db.Saches, "MaSach", "TenSach", hoaDonBanSach.MaSach);
            return View(hoaDonBanSach);
        }

        // GET: HoaDonBanSaches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDonBanSach hoaDonBanSach = db.HoaDonBanSaches.Find(id);
            if (hoaDonBanSach == null)
            {
                return HttpNotFound();
            }
            return View(hoaDonBanSach);
        }

        // POST: HoaDonBanSaches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HoaDonBanSach hoaDonBanSach = db.HoaDonBanSaches.Find(id);
            db.HoaDonBanSaches.Remove(hoaDonBanSach);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
