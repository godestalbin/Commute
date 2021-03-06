﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Commute.Properties;
using System.Web.Mvc;

namespace Commute.Models
{
    public class User : Entity
    {
        [Display(Name = "Account", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        public string Account { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Properties.Resources))]
        [StringLength(40, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        public string Name { get; set; }

        [Display(Name = "Password", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Email", ResourceType = typeof(Properties.Resources))]
        [Required]
        [StringLength(100)]
        [RegularExpression(".+@.+\\..+", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_invalid_mail")]
        public string EmailAddress { get; set; }
        
        public string PictureVersion { get; set; }

        public Nullable<decimal> LocationLatitude { get; set; }
        public Nullable<decimal> LocationLongitude { get; set; }
    }

    //Class to register new account
    public class RegisterUser : Entity
    {
        [Display(Name = "Account", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        public string Account { get; set; }
        [Display(Name = "Password", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Password_confirm", ResourceType = typeof(Properties.Resources))]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmedPassword { get; set; }
        [Display(Name = "Email", ResourceType = typeof(Properties.Resources))]
        [Required]
        [StringLength(100)]
        [RegularExpression(".+@.+\\..+", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_invalid_mail")]
        public string EmailAddress { get; set; }
    }

    //Class to change password
    public class UserPassword : Entity
    {
        [Display(Name = "Account", ResourceType = typeof(Properties.Resources))]
        public string Account { get; set; }
        [Display(Name = "Password_old", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Display(Name = "Password", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Mandatory")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Password_confirm", ResourceType = typeof(Properties.Resources))]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Error_string_too_long")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmedPassword { get; set; }
    }
}