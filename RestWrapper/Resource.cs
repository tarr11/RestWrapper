﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWrapper {
	public abstract class Resource {
		public Client Client { get; set; }
		public Resource(Client client) {
			this.Client = client;
		}

	}
}
