﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Core.Models;

namespace Microsoft.Health.Fhir.Core.Features.Search.Expressions
{
    /// <summary>
    /// Represents an include expression (where an additional resource is included based on a reference)
    /// </summary>
    public class IncludeExpression : Expression
    {
        private IReadOnlyCollection<string> _requires;
        private IReadOnlyCollection<string> _produces;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludeExpression"/> class.
        /// </summary>
        /// <param name="resourceType">The resource that supports the reference.</param>
        /// <param name="referenceSearchParameter">THe search parameter that establishes the reference relationship.</param>
        /// <param name="sourceResourceType">The source type of the reference.</param>
        /// <param name="targetResourceType">The target type of the reference.</param>
        /// <param name="referencedTypes">All the resource types referenced by resourceType</param>
        /// <param name="wildCard">If this is a wildcard reference include (include all referenced resources).</param>
        /// <param name="reversed">If this is a reversed include (revinclude) expression.</param>
        /// <param name="iterate"> If :iterate (:recurse) modifer was applied.</param>
        public IncludeExpression(string resourceType, SearchParameterInfo referenceSearchParameter, string sourceResourceType, string targetResourceType, IEnumerable<string> referencedTypes, bool wildCard, bool reversed, bool iterate)
        {
            EnsureArg.IsNotNullOrWhiteSpace(resourceType, nameof(resourceType));

            if (!wildCard)
            {
                EnsureArg.IsNotNull(referenceSearchParameter, nameof(referenceSearchParameter));
            }

            if (reversed)
            {
                EnsureArg.IsNotNull(sourceResourceType, nameof(sourceResourceType));
            }

            ResourceType = resourceType;
            ReferenceSearchParameter = referenceSearchParameter;
            SourceResourceType = sourceResourceType;
            TargetResourceType = targetResourceType;
            ReferencedTypes = referencedTypes?.ToList();
            WildCard = wildCard;
            Reversed = reversed;
            Iterate = iterate;
        }

        /// <summary>
        /// Gets the resource type which is being searched.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the reference search parameter for the relationship.
        /// </summary>
        public SearchParameterInfo ReferenceSearchParameter { get; }

        /// <summary>
        /// Gets the source resource type. Value will be null if none are specified.
        /// </summary>
        public string SourceResourceType { get; }

        /// <summary>
        /// Gets the target resource type. Value will be null if none are specified.
        /// </summary>
        public string TargetResourceType { get; }

        /// <summary>
        ///  Gets the type of resources referenced by resourceType. Used when iterating over wildcard results.
        /// </summary>
        public IReadOnlyCollection<string> ReferencedTypes { get; }

        /// <summary>
        ///  Gets the type of resources the expression requires (includes from).
        /// </summary>
        public IReadOnlyCollection<string> Requires => _requires ??= GetRequiredResources();

        /// <summary>
        ///  Gets the type of resources the expression produces.
        /// </summary>
        public IReadOnlyCollection<string> Produces => _produces ??= GetProducedResources();

        /// <summary>
        /// Gets if the include is a wildcard include.
        /// </summary>
        public bool WildCard { get; }

        /// <summary>
        /// Get if the expression is reversed.
        /// </summary>
        public bool Reversed { get; }

        /// <summary>
        /// Gets if the include has :iterate (:recurse) modifier.
        /// </summary>
        public bool Iterate { get; }

        public override TOutput AcceptVisitor<TContext, TOutput>(IExpressionVisitor<TContext, TOutput> visitor, TContext context)
        {
            EnsureArg.IsNotNull(visitor, nameof(visitor));

            return visitor.VisitInclude(this, context);
        }

        public override string ToString()
        {
            var targetType = TargetResourceType != null ? $":{TargetResourceType}" : string.Empty;
            var iterate = Iterate ? " Iterate" : string.Empty;
            var reversed = Reversed ? "Reversed " : string.Empty;
            var wildcard = WildCard ? " Wildcard" : string.Empty;
            var paramName = ReferenceSearchParameter != null ? $" {ReferenceSearchParameter.Name}" : string.Empty;
            return $"({reversed}Include{iterate}{wildcard}{paramName}{targetType})";
        }

        private IReadOnlyCollection<string> GetRequiredResources()
        {
            if (Reversed)
            {
                if (TargetResourceType != null)
                {
                    return new List<string> { TargetResourceType };
                }
                else if (ReferenceSearchParameter?.TargetResourceTypes != null && ReferenceSearchParameter.TargetResourceTypes.Any())
                {
                    return ReferenceSearchParameter.TargetResourceTypes;
                }
                else if (WildCard)
                {
                    return ReferencedTypes;
                }

                // impossible case
                return new List<string>();
            }
            else
            {
                return new List<string> { SourceResourceType };
            }
        }

        private IReadOnlyCollection<string> GetProducedResources()
        {
            if (Reversed)
            {
                return new List<string> { SourceResourceType };
            }
            else
            {
                if (TargetResourceType != null)
                {
                    return new List<string> { TargetResourceType };
                }
                else if (ReferenceSearchParameter?.TargetResourceTypes != null && ReferenceSearchParameter.TargetResourceTypes.Any())
                {
                    return ReferenceSearchParameter.TargetResourceTypes;
                }
                else if (WildCard)
                {
                    return ReferencedTypes;
                }

                // impossible case
                return new List<string>();
            }
        }
    }
}
