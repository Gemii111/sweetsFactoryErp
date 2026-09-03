using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryX.Application.DTOs.Requests.ProductRequests;

public sealed record InsertProductRequest(
	string Name,
	string Code,
	string Description);
