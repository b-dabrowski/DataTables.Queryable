﻿using System;
using System.Web.Mvc;

using Newtonsoft.Json;

using DataTables.Queryable.Samples.Database;
using DataTables.Queryable.Samples.Models;

namespace DataTables.Queryable.Samples.Controllers
{
    public class DataTablesController : Controller
    {
        /// <summary>
        /// Basic usage sample
        /// </summary>
        public JsonResult Sample1()
        {
            var request = new DataTablesRequest<Person>(Request.QueryString);
            using (var ctx = new DatabaseContext())
            {
                var persons = ctx.Persons.ToPagedList(request);
                return JsonDataTable(persons);
            }
        }

        /// <summary>
        /// Custom search predicate
        /// </summary>
        public JsonResult Sample2()
        {
            var request = new DataTablesRequest<Person>(Request.QueryString);

            request.Columns[p => p.Name]
                .GlobalSearchPredicate = (p) => p.Name.StartsWith(request.GlobalSearchValue);

            request.Columns[p => p.Position]
                .GlobalSearchPredicate = (p) => p.Position.StartsWith(request.GlobalSearchValue);

            request.Columns[p => p.Office]
                .GlobalSearchPredicate = (p) => p.Office.StartsWith(request.GlobalSearchValue);

            using (var ctx = new DatabaseContext())
            {
                var persons = ctx.Persons.ToPagedList(request);
                return JsonDataTable(persons);
            }
        }

        /// <summary>
        /// Custom filtering with disabled search
        /// </summary>
        public JsonResult Sample3()
        {
            var request = new DataTablesRequest<Person>(Request.QueryString);

            // get custom parameters from the request
            const string dateFormat = "yyyy-MM-dd";
            DateTime dateFrom = DateTime.ParseExact(request["dateFrom"], dateFormat, null);
            DateTime dateTo = DateTime.ParseExact(request["dateTo"], dateFormat, null);

            // define custom predicate to filter results:
            // get all persons hired in period between "dateFrom" and "dateTo"
            request.CustomFilterPredicate = 
                (s) => s.StartDate >= dateFrom && s.StartDate <= dateTo;

            using (var ctx = new DatabaseContext())
            {
                var persons = ctx.Persons.ToPagedList(request);
                return JsonDataTable(persons);
            }
        }

        /// <summary>
        /// Helper method that converts <see cref="IPagedList{T}"/> collection to the JSON-serialized object in datatables-friendly format.
        /// </summary>
        /// <param name="model"><see cref="IPagedList{T}"/> collection of items</param>
        /// <returns>JsonNetResult instance to be sent to datatables</returns>
        protected JsonNetResult JsonDataTable<T>(IPagedList<T> model)
        {
            JsonNetResult jsonResult = new JsonNetResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = new
            {
                recordsTotal = model.TotalCount,
                recordsFiltered = model.TotalCount,
                data = model
            };
            return jsonResult;
        }

        /// <summary>
        /// JsonNet-based JsonResult
        /// </summary>
        protected class JsonNetResult : JsonResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                var response = context.HttpContext.Response;

                response.ContentType = !String.IsNullOrEmpty(ContentType)
                    ? ContentType
                    : "application/json";

                if (ContentEncoding != null)
                    response.ContentEncoding = ContentEncoding;

                var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);
                response.Write(serializedObject);
            }
        }
    }
}