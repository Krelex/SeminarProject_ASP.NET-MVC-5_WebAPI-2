﻿using SeminarMVC.Models;
using SeminarMVC.Models.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SeminarMVC.Controllers
{
    [HandleError]
    public class UpisiController : Controller
    {
        SeminarREST clientSeminar = new SeminarREST();
        PredbiljezbaREST clientPredbiljezba = new PredbiljezbaREST();
        int pageSize = 5;

        // GET: Upisi
        public async Task<ActionResult> Index(int? active , int page = 1 )
        {
            SeminarViewModel model = new SeminarViewModel();

            if (active == 0)
            {
                ViewBag.Active = "nonactive";
                var all = await clientSeminar.GetAllAsync();
                var chekMe = all.Where(p => p.Popunjen == false).OrderByDescending(p => p.Datum).Skip((page - 1) * pageSize).Take(pageSize);
                model.seminar = chekMe;

                model.pagingInfo = new PagingInfo()
                {
                    TotalItems = all.Where(s => s.Popunjen == false).Count(),
                    ItemPerPage = pageSize,
                    CurrentPage = page
                };

                return View(model);

            }
            else if (active == 1)
            {
                ViewBag.Active = "active";
                var all = await clientSeminar.GetAllAsync();
                var checkMe = all.Where(p => p.Popunjen == true).OrderByDescending(p => p.Datum).Skip((page - 1) * pageSize).Take(pageSize);
                model.seminar = checkMe;

                model.pagingInfo = new PagingInfo()
                {
                    TotalItems = all.Where(s => s.Popunjen == true).Count(),
                    ItemPerPage = pageSize,
                    CurrentPage = page
                };

                return View(model);
            }
            else
            {
                ViewBag.Active = "all";
                var all = await clientSeminar.GetAllAsync();
                var checkMe = all.OrderByDescending(p => p.Datum).Skip((page - 1) * pageSize).Take(pageSize);
                model.seminar = checkMe;

                model.pagingInfo = new PagingInfo()
                {
                    TotalItems = all.Count(),
                    ItemPerPage = pageSize,
                    CurrentPage = page
                };

                return View(model);
            }
        }

        public async Task<ActionResult> Search(string key)
        {
            SeminarViewModel model = new SeminarViewModel();
            var found = await clientSeminar.SearchAsync(key);

            if (found.Count() == 0)
            {
                throw new ArgumentException("Nažalost unjeli ste netočne podatke :( ");
            }

            @ViewBag.Key = key;
            model.seminar = found;

            return View("Index", model);

        }

        public async Task<ActionResult> Korisnika(int id, string name)
        {
            ViewBag.SeminarId = id;
            ViewBag.Name = name;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Korisnika(Predbiljezba predbiljezba)
        {
            predbiljezba.Active = false;

            predbiljezba.Datum = DateTime.Now;

            var SeminarId =int.Parse(Request.Form["IdSeminar"]);
            predbiljezba.IdSeminar = SeminarId;




            if (ModelState.IsValid)
            {
                await clientPredbiljezba.CreateAsync(predbiljezba);
                var seminari = await clientSeminar.GetByIdAsync(SeminarId);

                var check = seminari.PopunjenCheck();

                TempData["Sucess"] = seminari.Naziv;
                TempData["Naziv"] = predbiljezba.Ime;

                return RedirectToAction("Index");
            }
            ViewBag.Error = "Desila se pogreska, molimo vas pokusajte ponovo !";
            return View();
        }
    }
}