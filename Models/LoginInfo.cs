﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinanzCSU.Models;

[Table("LoginInfo")]
public partial class LoginInfo
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Letters no numbers/symbols")]
    public string UName { get; set; }

    [Required]
    [StringLength(255)]
    [RegularExpression(@"^[a-zA-Z0-9\*\$]+$", ErrorMessage = "Letters, digits, *, $")]
    public string UPass { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z]+\s[a-zA-Z]+$", ErrorMessage = "First and Last Name; Upper and lower case letters")]
    public string FullName { get; set; }

    [StringLength(50)]
    [Display(Name = "Full Name", Prompt = "First and Last Name")]
    public string URole { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserBudget> UserBudgets { get; set; } = new List<UserBudget>();
}