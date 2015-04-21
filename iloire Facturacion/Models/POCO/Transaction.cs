﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


	public class Transaction
	{
		[Key]
		public int TransactionID { get; set; }
		public int UserID { get; set; }
		[ForeignKey("Product")]
		public int ProductID { get; set; }

		[ForeignKey("Customer")]
		public int CustomerID { get; set; }

		public DateTime TimeStamp { get; set; }
		public virtual User User { get; set; }
		public virtual Product Product { get; set; }
		public virtual Customer Customer { get; set; }
	}
