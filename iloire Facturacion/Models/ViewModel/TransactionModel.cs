﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[NotMapped]
public class TransactionModel : Transaction
{
	/// <summary>
	/// total amount for this transaction
	/// </summary>
	[DataType(DataType.Currency)]
	[DisplayFormat(DataFormatString = "{0:C}")]
	public decimal? TransactionSellAmount { get; set; }
}