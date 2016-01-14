﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BVNetwork.Attend.Business.API;
using BVNetwork.Attend.Business.Participant;
using BVNetwork.Attend.Models.Blocks;
using BVNetwork.Attend.Models.Pages;
using BVNetwork.Attend.Models.ViewModels;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Web;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.XForms;
using EPiServer.Web.Routing;
using EPiServer.XForms.Util;

namespace BVNetwork.Attend.Controllers
{
    [TemplateDescriptor(TemplateTypeCategory = TemplateTypeCategories.MvcPartialController, Inherited =true)]
    public class EventPageBasePartialController : PageController<EventPageBase>
    {
        private XFormPageUnknownActionHandler _xformHandler;
        private IContentRepository _contentRepository;

        public EventPageBasePartialController(XFormPageUnknownActionHandler xformHandler)
        {
            _xformHandler = xformHandler;
            _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        }

        public ActionResult Index(EventPageBase currentPage, string contentLink)
        {
            /* Implementation of action. You can create your own view model class that you pass to the view or
             * you can pass the page type for simpler templates */
            //var model =
            //if(Request.HttpMethod =="POST")

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            var viewModel =  CreateEventRegistrationModel(currentPage, contentLink);

            var pageRouteHelper = ServiceLocator.Current.GetInstance<PageRouteHelper>();
            PageData hostPageData = pageRouteHelper.Page;

            if (currentPage != null && hostPageData != null)
            {
                

                var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
                var pageUrl = urlResolver.GetUrl(hostPageData.ContentLink);

                var actionUrl = string.Format("{0}/", pageUrl);
                actionUrl = UriSupport.AddQueryString(actionUrl, "XFormId", currentPage.RegistrationForm.Id.ToString());
                actionUrl = UriSupport.AddQueryString(actionUrl, "failedAction", "Failed");
                actionUrl = UriSupport.AddQueryString(actionUrl, "successAction", "Success");

                viewModel.ActionUrl = actionUrl;
            }

            //return PartialView(viewModel);
            return PartialView("~/modules/BVNetwork.Attend/Views/Pages/Partials/EventPagePartial.cshtml", viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Index(EventPageBase currentPage, XFormPostedData xFormPostedData, string contentLink)
        {
            var model = CreateEventRegistrationModel(currentPage, contentLink);

            string participantEmail = "";
            foreach (var fragment in xFormPostedData.Fragments)
            {
                if (fragment as EPiServer.XForms.Parsing.InputFragment != null && ((fragment as EPiServer.XForms.Parsing.InputFragment).Reference.ToLower() == "epost" || (fragment as EPiServer.XForms.Parsing.InputFragment).Reference.ToLower() == "email"))
                {
                    participantEmail = (fragment as EPiServer.XForms.Parsing.InputFragment).Value;
                }
            }

            string xformdata = new EPiServer.Web.Mvc.XForms.XFormPageHelper().GetXFormData(this, xFormPostedData).Data.InnerXml;
            IParticipant participant = AttendRegistrationEngine.GenerateParticipation(model.EventPageBase.ContentLink, participantEmail, true, xformdata, "Participant submitted form");
            participant.XForm = xformdata;

            //Add sessions to participant

            participant.Sessions = new ContentArea();
            foreach (var key in Request.Form.AllKeys)
            {
                if (key.StartsWith("Session"))
                {
                    var sessionContentReference = new ContentReference(Request[key]);
                    participant.Sessions.Items.Add(new ContentAreaItem() { ContentLink = sessionContentReference });
                }
            }
            _contentRepository.Save(participant as IContent, SaveAction.Publish, AccessLevel.NoAccess);

            model.Submitted = participant.AttendStatus.ToLower() == "submitted";

            ViewBag.Participant = participant;


            //return RedirectToAction("Success");
            return PartialView("~/modules/BVNetwork.Attend/Views/Pages/Partials/EventPagePartialSuccess.cshtml", model);
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Success(EventPageBase currentPage, XFormPostedData xFormPostedData, string contentLink)
        //{
        //    Model model = null;
        //    return PartialView("Success", model);
        //}


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Failed(EventPageBase currentPage, XFormPostedData xFormPostedData, string contentLink)
        {
            var model = CreateEventRegistrationModel(currentPage, contentLink);
            return View("Index", model);
        }





        private EventRegistrationModel CreateEventRegistrationModel(EventPageBase currentPage, string contentLink)
        {
            var model = new EventRegistrationModel(currentPage);
            var repository = ServiceLocator.Current.GetInstance<IContentLoader>();
            var localizationService = ServiceLocator.Current.GetInstance<LocalizationService>();

            var pageRouteHelper = EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<EPiServer.Web.Routing.PageRouteHelper>();
            PageReference currentPageLink = pageRouteHelper.PageLink;
            model.HostPageData = pageRouteHelper.Page;
            model.EventPageBase = currentPage;
            model.Sessions = BVNetwork.Attend.Business.API.AttendSessionEngine.GetSessionsList(model.EventPageBase.PageLink);
            model.AvalibleSeats = AttendRegistrationEngine.GetAvailableSeats(model.EventPageBase.PageLink);
            model.PriceText = model.EventPageBase.EventDetails.Price > 0 ? model.EventPageBase.EventDetails.Price + " " + localizationService.GetString("/eventRegistrationTemplate/norwegianCurrencey") : localizationService.GetString("/eventRegistrationTemplate/freeOfCharge");
            return model;
        }

    }
}