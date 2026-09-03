using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryX.Application.DTOs.Responses.UserManagementResponses;

public sealed record GetUserProfileResponse(int Id, string UserName, string? FullName, string? Email);
