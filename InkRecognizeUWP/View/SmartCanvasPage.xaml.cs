﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Input.Inking;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace InkRecognizeUWP.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SmartCanvasPage : Page
    {
        // Global variable
        DispatcherTimer recoTimer;
        InkAnalyzer inkAnalyzer;
        InkRecognizerContainer inkRecognizerContainer;

        public SmartCanvasPage()
        {
            this.InitializeComponent();
            InkCanvasInit();
            RecognitionTimerInit();
            ComboButtonInit();
        }

        #region Initialize
        private void RecognitionTimerInit()
        {
            recoTimer = new DispatcherTimer();
            recoTimer.Interval = TimeSpan.FromSeconds(1);
            recoTimer.Tick += RecoTimer_TickAsync;
        }

        private void InkCanvasInit()
        {
            myInkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | 
                                                        CoreInputDeviceTypes.Touch | 
                                                        CoreInputDeviceTypes.Pen;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes
            {
                Color = Colors.White,
                IgnorePressure = false,
                FitToCurve = true
            };
            myInkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkAnalyzer = new InkAnalyzer();

            myInkCanvas.InkPresenter.StrokesCollected += OnStrokesCollected;
            myInkCanvas.InkPresenter.StrokeInput.StrokeStarted += (sender, e) => recoTimer.Stop();

        }

        private void ComboButtonInit()
        {
            inkRecognizerContainer = new InkRecognizerContainer();
            IReadOnlyList<InkRecognizer> installedRecognizers = inkRecognizerContainer.GetRecognizers();
            // inkRecognizerContainer is null if a recognition engine is not available.
            if (!(inkRecognizerContainer == null))
            {
                this.installedRecognizers.ItemsSource = installedRecognizers;
                this.installedRecognizers.SelectedIndex = 0;
                inkRecognizerContainer.SetDefaultRecognizer(installedRecognizers[0]);
            }
        }
        #endregion

        #region Events
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            recoTimer.Stop();
        }

        private void OnRecognizersSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inkRecognizerContainer.SetDefaultRecognizer(
                inkRecognizerContainer.GetRecognizers()[this.installedRecognizers.SelectedIndex]);
        }

        private void OnStrokesCollected(InkPresenter presenter, InkStrokesCollectedEventArgs args)
        {
            recoTimer.Stop();
            // If you're only interested in a specific type of recognition,
            // such as writing or drawing, you can constrain recognition 
            // using the SetStrokeDataKind method, which can improve both 
            // efficiency and recognition results.
            // In this example, "InkAnalysisStrokeKind.Writing" is used.
            //foreach (var stroke in args.Strokes)
            //{
            //    inkAnalyzer.AddDataForStroke(stroke);
            //    inkAnalyzer.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
            //}
            recoTimer.Start();
        }
        #endregion

        #region Analyze
        private async void RecoTimer_TickAsync_Old(object sender, object e)
        {
            recoTimer.Stop();
            if (!inkAnalyzer.IsAnalyzing)
            {
                var inkStrokes = myInkCanvas.InkPresenter.StrokeContainer.GetStrokes();
                // Ensure an ink stroke is present.
                if (inkStrokes.Count > 0)
                {
                    inkAnalyzer.AddDataForStrokes(inkStrokes);

                    // In this example, we try to recognizing both 
                    // writing and drawing, so the platform default 
                    // of "InkAnalysisStrokeKind.Auto" is used.
                    // If you're only interested in a specific type of recognition,
                    // such as writing or drawing, you can constrain recognition 
                    // using the SetStrokeDataKind method as follows:
                    // foreach (var stroke in strokesText)
                    // {
                    //     analyzerText.SetStrokeDataKind(
                    //      stroke.Id, InkAnalysisStrokeKind.Writing);
                    // }
                    // This can improve both efficiency and recognition results.
                    var inkAnalysisResults = await inkAnalyzer.AnalyzeAsync();

                    // Have ink strokes on the canvas changed?
                    if (inkAnalysisResults.Status == InkAnalysisStatus.Updated)
                    {
                        // Find all strokes that are recognized as handwriting and 
                        // create a corresponding ink analysis InkWord node.
                        var inkWordNodes =
                            inkAnalyzer.AnalysisRoot.FindNodes(
                                InkAnalysisNodeKind.InkWord);

                        // Iterate through each InkWord node.
                        // Draw primary recognized text on recognitionCanvas 
                        // (for this example, we ignore alternatives), and delete 
                        // ink analysis data and recognized strokes.
                        foreach (InkAnalysisInkWord node in inkWordNodes)
                        {
                            // Draw a TextBlock object on the recognitionCanvas.
                            DrawText(node.RecognizedText, node.BoundingRect);

                            // Display recognition result
                            bottomLabel.Text = node.RecognizedText;

                            foreach (var strokeId in node.GetStrokeIds())
                            {
                                var stroke =
                                    myInkCanvas.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                                stroke.Selected = true;
                            }
                            inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
                        }
                        myInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();

                        // Find all strokes that are recognized as a drawing and 
                        // create a corresponding ink analysis InkDrawing node.
                        var inkDrawingNodes =
                            inkAnalyzer.AnalysisRoot.FindNodes(
                                InkAnalysisNodeKind.InkDrawing);
                        // Iterate through each InkDrawing node.
                        // Draw recognized shapes on recognitionCanvas and
                        // delete ink analysis data and recognized strokes.
                        foreach (InkAnalysisInkDrawing node in inkDrawingNodes)
                        {
                            if (node.DrawingKind == InkAnalysisDrawingKind.Drawing)
                            {
                                // Catch and process unsupported shapes (lines and so on) here.
                            }
                            // Process generalized shapes here (ellipses and polygons).
                            else
                            {
                                // Draw an Ellipse object on the recognitionCanvas (circle is a specialized ellipse).
                                if (node.DrawingKind == InkAnalysisDrawingKind.Circle || node.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                                {
                                    DrawEllipse(node);
                                }
                                // Draw a Polygon object on the recognitionCanvas.
                                else
                                {
                                    DrawPolygon(node);
                                }
                                foreach (var strokeId in node.GetStrokeIds())
                                {
                                    var stroke = myInkCanvas.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                                    stroke.Selected = true;
                                }
                            }
                            inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
                        }
                        myInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                    }
                }
            }
            else
            {
                // Ink analyzer is busy. Wait a while and try again.
                recoTimer.Start();
            }
        }

        private async void RecoTimer_TickAsync(object sender, object e)
        {
            recoTimer.Stop();
            if (!inkAnalyzer.IsAnalyzing)
            {
                var currentStrokes = myInkCanvas.InkPresenter.StrokeContainer.GetStrokes();
                // Ensure an ink stroke is present.
                if (currentStrokes.Count <= 0)
                {
                    return;
                }

                if (inkRecognizerContainer == null)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "No recognition engine found",
                        CloseButtonText = "Cancel",
                        Content = new TextBlock()
                        {
                            Text = "You must install handwriting recognition engine.",
                            Margin = new Thickness(0, 4, 4, 4)
                        },
                    };
                    await contentDialog.ShowAsync();

                    return;
                }

                // Find all strokes that are recognized as a drawing and 
                // create a corresponding ink analysis InkDrawing node.
                inkAnalyzer.AddDataForStrokes(currentStrokes);
                var inkAnalysisResults = await inkAnalyzer.AnalyzeAsync();
                var inkDrawingNodes =
                    inkAnalyzer.AnalysisRoot.FindNodes(
                        InkAnalysisNodeKind.InkDrawing);
                // Iterate through each InkDrawing node.
                // Draw recognized shapes on recognitionCanvas and
                // delete ink analysis data and recognized strokes.
                foreach (InkAnalysisInkDrawing node in inkDrawingNodes)
                {
                    if (node.DrawingKind == InkAnalysisDrawingKind.Drawing)
                    {
                        // Catch and process unsupported shapes (lines and so on) here.
                    }
                    // Process generalized shapes here (ellipses and polygons).
                    else
                    {
                        // Draw an Ellipse object on the recognitionCanvas (circle is a specialized ellipse).
                        if (node.DrawingKind == InkAnalysisDrawingKind.Circle || node.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                        {
                            DrawEllipse(node);
                        }
                        // Draw a Polygon object on the recognitionCanvas.
                        else
                        {
                            DrawPolygon(node);
                        }
                        foreach (var strokeId in node.GetStrokeIds())
                        {
                            var stroke = myInkCanvas.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                            stroke.Selected = true;
                        }
                    }
                    inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
                }
                myInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();

                if (myInkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count <= 0)
                {
                    return;
                }

                // Recognize and draw text
                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(
                    myInkCanvas.InkPresenter.StrokeContainer,
                    InkRecognitionTarget.All);

                if (recognitionResults.Count <= 0)
                {
                    return;
                }
                var candidate = recognitionResults.First().GetTextCandidates();
                bottomLabel.Text = candidate.First();
                DrawText(candidate.First(), recognitionResults.First().BoundingRect);

                // Delete all strokes
                foreach (var stroke in recognitionResults.First().GetStrokes())
                {
                    stroke.Selected = true;
                }
                myInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }
            else
            {
                // Ink analyzer is busy. Wait a while and try again.
                recoTimer.Start();
            }
        }
        #endregion

        #region Draw Shapes and Text
        /// <summary>
        /// Draw ink recognition text string on the recognitionCanvas.
        /// </summary>
        /// <param name="recognizedText">The string returned by text recognition.</param>
        /// <param name="boundingRect">The bounding rect of the original ink writing.</param>
        private void DrawText(string recognizedText, Rect boundingRect)
        {
            TextBlock text = new TextBlock();
            Canvas.SetTop(text, boundingRect.Top);
            Canvas.SetLeft(text, boundingRect.Left);

            text.Text = recognizedText;
            text.FontSize = boundingRect.Height;

            recognitionCanvas.Children.Add(text);
        }

        // Draw an ellipse on the recognitionCanvas.
        private void DrawEllipse(InkAnalysisInkDrawing shape)
        {
            var points = shape.Points;
            Ellipse ellipse = new Ellipse();

            ellipse.Width = shape.BoundingRect.Width;
            ellipse.Height = shape.BoundingRect.Height;

            Canvas.SetTop(ellipse, shape.BoundingRect.Top);
            Canvas.SetLeft(ellipse, shape.BoundingRect.Left);

            var brush = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 0, 0, 255));
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 2;
            recognitionCanvas.Children.Add(ellipse);
        }

        // Draw a polygon on the recognitionCanvas.
        private void DrawPolygon(InkAnalysisInkDrawing shape)
        {
            List<Point> points = new List<Point>(shape.Points);
            Polygon polygon = new Polygon();

            foreach (Point point in points)
            {
                polygon.Points.Add(point);
            }

            var brush = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 0, 0, 255));
            polygon.Stroke = brush;
            polygon.StrokeThickness = 2;
            recognitionCanvas.Children.Add(polygon);
        }
        #endregion
    }
}
