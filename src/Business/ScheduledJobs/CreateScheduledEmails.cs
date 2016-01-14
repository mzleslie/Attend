﻿using BVNetwork.Attend.Business.Email;
using BVNetwork.Attend.Business.Text;
using BVNetwork.Attend.Models.Blocks;
using BVNetwork.Attend.Models.Pages;
using EPiServer;
using EPiServer.BaseLibrary.Scheduling;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Filters;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;

namespace BVNetwork.Attend.Business.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "CreateScheduledEmails")]
    public class CreateScheduledEmails : JobBase
    {
        private bool _stopSignaled;

        public CreateScheduledEmails()
        {
            IsStoppable = true;
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        /// Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {


            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));
            PropertyCriteriaCollection criteria = new PropertyCriteriaCollection();
            PropertyCriteria pageTypeCriteria = new PropertyCriteria();
            pageTypeCriteria.Condition = CompareCondition.Equal;
            pageTypeCriteria.Value = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<EventPage>().ID.ToString();
            pageTypeCriteria.Type = PropertyDataType.PageType;
            pageTypeCriteria.Name = "PageTypeID";
            pageTypeCriteria.Required = true;
            criteria.Add(pageTypeCriteria);
            PageDataCollection allEvents = new PageDataCollection(); 
            
            PageDataCollection allLanguages = DataFactory.Instance.GetLanguageBranches(PageReference.StartPage);


            foreach (PageData pageData in allLanguages)
            {
                allEvents.Add(DataFactory.Instance.FindPagesWithCriteria(PageReference.RootPage, criteria, pageData.LanguageBranch));
                
            }
            int cnt = 0;
            foreach (var eventPage in allEvents)
            {
                cnt += UpdateEvent(eventPage as EventPage);
            }

            return "Found " + allEvents.Count + " pages, converted "+cnt+"e-mail templates!";

            //Add implementation

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if (_stopSignaled)
            {
                return "Stop of job was called";
            }

            return "Change to message that describes outcome of execution";
        }

        public static bool ContainsStatusMail(IEnumerable<ScheduledEmailBlock> emails, AttendStatus status)
        {
            foreach (var scheduledEmailBlock in emails)
            {
                if (scheduledEmailBlock.EmailSendOptions == SendOptions.Action &&
                    scheduledEmailBlock.SendOnStatus == status)
                    return true;
            }
            return false;

        }

        public static int UpdateEvent(EventPage eventPage)
        {
            var scheduledEmails =
                Attend.Business.API.AttendScheduledEmailEngine.GetScheduledEmails(eventPage.ContentLink);
            int cnt = 0;
            if (!ContainsStatusMail(scheduledEmails, AttendStatus.Submitted))
            {
                cnt++;
                CreateScheduledEmail(eventPage, AttendStatus.Submitted, eventPage.SubmitMailTemplate, eventPage.SubmitMailTemplateBlock, "Submit mail template");
            }

            if (!ContainsStatusMail(scheduledEmails, AttendStatus.Confirmed))
            {
                cnt++;
                CreateScheduledEmail(eventPage, AttendStatus.Confirmed, eventPage.ConfirmMailTemplate, eventPage.ConfirmMailTemplateBlock, "Confirm mail template");
            }

            if (!ContainsStatusMail(scheduledEmails, AttendStatus.Cancelled))
            {
                cnt++;
                CreateScheduledEmail(eventPage, AttendStatus.Cancelled, eventPage.CancelMailTemplate, eventPage.CancelMailTemplateBlock, "Cancel mail template");
            }

            return cnt;

        }

        public static void CreateScheduledEmail(EventPage eventPage, AttendStatus status, EmailTemplateBlock emailTemplate, ContentReference emailTemplateContentReference, string name)
        {
            ScheduledEmailBlock emailBlock =
                API.AttendScheduledEmailEngine.GenerateScheduledEmailBlock(eventPage.ContentLink).CreateWritableClone() as ScheduledEmailBlock;
            emailBlock.EmailSendOptions = SendOptions.Action;
            emailBlock.SendOnStatus = status;
            emailBlock.EmailTemplate.BCC = emailTemplate.BCC;
            emailBlock.EmailTemplate.CC = emailTemplate.CC;
            emailBlock.EmailTemplate.From = emailTemplate.From;
            emailBlock.EmailTemplate.To = emailTemplate.To;
            emailBlock.EmailTemplate.Subject = emailTemplate.Subject;
            emailBlock.EmailTemplate.MainBody = emailTemplate.MainBody;
            emailBlock.EmailTemplate.MainTextBody = emailTemplate.MainTextBody;

            emailBlock.EmailTemplateContentReference = emailTemplateContentReference;

            (emailBlock as IContent).Name = name;

            DataFactory.Instance.Save(emailBlock as IContent, SaveAction.Publish);

        }
    }
}
