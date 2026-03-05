using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Reaction;
/// <summary>
/// Request to toggle a reaction on a target (Post or Comment).
/// </summary>
public sealed record ToggleReactionRequest(ReactionType ReactionType);
