﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using CommunityToolkit.WinUI.UI.Media.Geometry.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace CommunityToolkit.WinUI.UI.Media.Geometry.Elements.Brush
{
    /// <summary>
    /// Represents a CanvasLinearGradientBrush with GradientStopHdrs
    /// </summary>
    internal sealed class LinearGradientHdrBrushElement : AbstractCanvasBrushElement
    {
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private CanvasAlphaMode _alphaMode;
        private CanvasBufferPrecision _bufferPrecision;
        private CanvasEdgeBehavior _edgeBehavior;
        private CanvasColorSpace _preInterpolationColorSpace;
        private CanvasColorSpace _postInterpolationColorSpace;
        private List<CanvasGradientStopHdr> _gradientStopHdrs;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearGradientHdrBrushElement"/> class.
        /// </summary>
        /// <param name="capture">Capture object</param>
        public LinearGradientHdrBrushElement(Capture capture)
        {
            // Set the default values
            _startPoint = Vector2.Zero;
            _endPoint = Vector2.Zero;
            _opacity = 1f;
            _alphaMode = (CanvasAlphaMode)0;
            _bufferPrecision = (CanvasBufferPrecision)0;
            _edgeBehavior = (CanvasEdgeBehavior)0;

            // Default ColorSpace is sRGB
            _preInterpolationColorSpace = CanvasColorSpace.Srgb;
            _postInterpolationColorSpace = CanvasColorSpace.Srgb;
            _gradientStopHdrs = new List<CanvasGradientStopHdr>();

            // Initialize
            Initialize(capture);
        }

        /// <summary>
        /// Creates the CanvasLinearGradientBrush from the parsed data
        /// </summary>
        /// <param name="resourceCreator">ICanvasResourceCreator object</param>
        /// <returns>Instance of <see cref="ICanvasBrush"/></returns>
        public override ICanvasBrush CreateBrush(ICanvasResourceCreator resourceCreator)
        {
            var brush = CanvasLinearGradientBrush.CreateHdr(
                resourceCreator,
                _gradientStopHdrs.ToArray(),
                _edgeBehavior,
                _alphaMode,
                _preInterpolationColorSpace,
                _postInterpolationColorSpace,
                _bufferPrecision);

            brush.StartPoint = _startPoint;
            brush.EndPoint = _endPoint;
            brush.Opacity = _opacity;

            return brush;
        }

        /// <summary>
        /// Gets the Regex for extracting Brush Element Attributes
        /// </summary>
        /// <returns>Regex</returns>
        protected override Regex GetAttributesRegex()
        {
            return RegexFactory.GetAttributesRegex(BrushType.LinearGradientHdr);
        }

        /// <summary>
        /// Gets the Brush Element Attributes from the Match
        /// </summary>
        /// <param name="match">Match object</param>
        protected override void GetAttributes(Match match)
        {
            // Start Point
            float.TryParse(match.Groups["StartX"].Value, out var startX);
            float.TryParse(match.Groups["StartY"].Value, out var startY);
            _startPoint = new Vector2(startX, startY);

            // End Point
            float.TryParse(match.Groups["EndX"].Value, out var endX);
            float.TryParse(match.Groups["EndY"].Value, out var endY);
            _endPoint = new Vector2(endX, endY);

            // Opacity (optional)
            var group = match.Groups["Opacity"];
            if (group.Success)
            {
                float.TryParse(group.Value, out _opacity);
            }

            // Alpha Mode (optional)
            group = match.Groups["AlphaMode"];
            if (group.Success)
            {
                Enum.TryParse(group.Value, out _alphaMode);
            }

            // Buffer Precision (optional)
            group = match.Groups["BufferPrecision"];
            if (group.Success)
            {
                Enum.TryParse(group.Value, out _bufferPrecision);
            }

            // Edge Behavior (optional)
            group = match.Groups["EdgeBehavior"];
            if (group.Success)
            {
                Enum.TryParse(group.Value, out _edgeBehavior);
            }

            // Pre Interpolation ColorSpace (optional)
            group = match.Groups["PreColorSpace"];
            if (group.Success)
            {
                Enum.TryParse(group.Value, out _preInterpolationColorSpace);
            }

            // Post Interpolation ColorSpace (optional)
            group = match.Groups["PostColorSpace"];
            if (group.Success)
            {
                Enum.TryParse(group.Value, out _postInterpolationColorSpace);
            }

            // GradientStopHdrs
            group = match.Groups["GradientStops"];
            if (group.Success)
            {
                _gradientStopHdrs.Clear();
                foreach (Capture capture in group.Captures)
                {
                    var gradientMatch = RegexFactory.GradientStopHdrRegex.Match(capture.Value);
                    if (!gradientMatch.Success)
                    {
                        continue;
                    }

                    // Main Attributes
                    var main = gradientMatch.Groups["Main"];
                    if (main.Success)
                    {
                        var mainMatch = RegexFactory.GetAttributesRegex(GradientStopAttributeType.MainHdr)
                                                    .Match(main.Value);
                        float.TryParse(mainMatch.Groups["Position"].Value, out float position);
                        float.TryParse(mainMatch.Groups["X"].Value, out float x);
                        float.TryParse(mainMatch.Groups["Y"].Value, out float y);
                        float.TryParse(mainMatch.Groups["Z"].Value, out float z);
                        float.TryParse(mainMatch.Groups["W"].Value, out float w);

                        var color = new Vector4(x, y, z, w);

                        _gradientStopHdrs.Add(new CanvasGradientStopHdr()
                        {
                            Color = color,
                            Position = position
                        });
                    }

                    // Additional Attributes
                    var additional = gradientMatch.Groups["Additional"];
                    if (!additional.Success)
                    {
                        continue;
                    }

                    foreach (Capture addCapture in additional.Captures)
                    {
                        var addMatch = RegexFactory.GetAttributesRegex(GradientStopAttributeType.AdditionalHdr)
                                                   .Match(addCapture.Value);
                        float.TryParse(addMatch.Groups["Position"].Value, out float position);
                        float.TryParse(addMatch.Groups["X"].Value, out float x);
                        float.TryParse(addMatch.Groups["Y"].Value, out float y);
                        float.TryParse(addMatch.Groups["Z"].Value, out float z);
                        float.TryParse(addMatch.Groups["W"].Value, out float w);

                        var color = new Vector4(x, y, z, w);

                        _gradientStopHdrs.Add(new CanvasGradientStopHdr()
                        {
                            Color = color,
                            Position = position
                        });
                    }
                }

                // Sort the stops based on their position
                if (_gradientStopHdrs.Any())
                {
                    _gradientStopHdrs = _gradientStopHdrs.OrderBy(g => g.Position).ToList();
                }
            }
        }
    }
}
