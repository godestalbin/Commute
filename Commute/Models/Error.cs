using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Commute.Models
{
    public class Error
    {
        [Display(Name = "Controller:", Description = "Controller:")]
        public string ControllerName { get; set; }
        [Display(Name = "Action:", Description = "Action:")]
        public string ActionName { get; set; }
        [Display(Name = "Error message:", Description = "Error message:")]
        public string Message { get; set; }

        public Error()
        {
        }

        public Error(string controllerName, string actionName, string message)
        {
            ControllerName = controllerName;
            ActionName = actionName;
            Message = message;
        }
    }
}