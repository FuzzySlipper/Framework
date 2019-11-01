using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PixelComrades {
    public partial class SpriteAnimatorWindow {

        private static readonly float TIMELINE_SCRUBBER_HEIGHT = 16;
        private static readonly float TIMELINE_EVENT_HEIGHT = 12;
        private static readonly float TIMELINE_BOTTOMBAR_HEIGHT = 18;
        private static readonly float TIMELINE_OFFSET_MIN = -10;

        private static readonly float SCRUBBER_INTERVAL_TO_SHOW_LABEL = 60.0f;
        private static readonly float SCRUBBER_INTERVAL_WIDTH_MIN = 10.0f;
        private static readonly float SCRUBBER_INTERVAL_WIDTH_MAX = 80.0f;

        private static readonly Color COLOR_UNITY_BLUE = new Color(0.3f, 0.5f, 0.85f, 1);

        private static readonly Color COLOR_INSERT_FRAMES_LINE = COLOR_UNITY_BLUE;
        private static readonly Color COLOR_EVENT_BAR_BG = new Color(0.2f, 0.2f, 0.2f);

        private static readonly Color COLOR_EVENT_LABEL_BG = COLOR_EVENT_BAR_BG*0.8f + Color.grey*0.2f;
            // Fake alpha'd look while stillmasking things behind it

        private static readonly Color COLOR_EVENT_LABEL_BG_SELECTED = COLOR_EVENT_BAR_BG*0.8f + COLOR_UNITY_BLUE*0.2f;

        private static readonly float FRAME_RESIZE_RECT_WIDTH = 8;

        //static readonly float EVENT_WIDTH = 2;
        private static readonly float EVENT_CLICK_OFFSET = -2;
        private static readonly float EVENT_CLICK_WIDTH = 10;

        private int _resizeFrameId;
        private float _selectionMouseStart;
        private float _timelineEventBarHeight = TIMELINE_EVENT_HEIGHT;

        private enum eDragState {
            None,
            Scrub,
            ResizeFrame,
            MoveFrame,
            SelectFrame,
            //MoveEvent,
            //SelectEvent
        }

        private float AnimTimeToGuiPos(Rect rect, float time) {
            return rect.xMin + _timelineOffset + time*_timelineScale;
        }

        // Clears both frame and event selection
        private void ClearSelection() {
            _selectedFrames.Clear();
            //_selectedEvents.Clear();
        }

        private List<int> CreateIntervalSizeList(out int intervalId) {
            var intervalSizes = new List<int>();
            var tmpSampleRate = (int) _clip.FramesPerSecond;
            while (true) {
                var div = 0;
                if (tmpSampleRate == 30) {
                    div = 3;
                }
                else if (tmpSampleRate%2 == 0) {
                    div = 2;
                }
                else if (tmpSampleRate%5 == 0) {
                    div = 5;
                }
                else if (tmpSampleRate%3 == 0) {
                    div = 3;
                }
                else {
                    break;
                }
                tmpSampleRate /= div;
                intervalSizes.Insert(0, div);
            }
            intervalId = intervalSizes.Count;
            intervalSizes.AddRange(new[] {
                5, 2, 3, 2,
                5, 2, 3, 2
            });
            return intervalSizes;
        }

        private float GuiPosToAnimTime(Rect rect, float mousePosX) {
            var pos = mousePosX - rect.xMin;
            return (pos - _timelineOffset)/_timelineScale;
        }

        private void LayoutBottomBar(Rect rect) {
            // Give bottom bar a min width
            rect = new Rect(rect) {
                width = MathEx.Max(rect.width, 655)
            };

            GUI.BeginGroup(rect, Styles.TIMELINE_BOTTOMBAR_BG);

            //if (_selectedEvents.Count == 1) {
            //    // Event data editor
            //    LayoutBottomBarEventData(rect);
            //}
            if (_selectedFrames.Count == 1) {
                // Animation Frame data editor
                LayoutBottomBarFrameData(rect);
            }

            GUI.EndGroup();
        }

        private void LayoutBottomBarFrameData(Rect rect) {
            var frame = _selectedFrames[0];

            EditorGUI.BeginChangeCheck();

            float xOffset = 10;
            float width = 60;
            GUI.Label(new Rect(xOffset, 1, width, rect.height), "Frame:", EditorStyles.boldLabel);

            // Function Name

            xOffset += width;
            width = 250;

            frame.Sprite =
                EditorGUI.ObjectField(new Rect(xOffset, 2, width, rect.height - 3), frame.Sprite, typeof(Sprite), false)
                    as Sprite;

            xOffset += width + 5;
            width = 50;
            // Frame length (in samples)
            EditorGUI.LabelField(new Rect(xOffset, 2, width, rect.height - 3), "Length");

            xOffset += width + 5;
            width = 30;

            GUI.SetNextControlName("Frame Length");
            var frameLen = Mathf.RoundToInt(frame.RealLength/GetFrameTime());
            frameLen = EditorGUI.IntField(new Rect(xOffset, 2, width, rect.height - 3), frameLen);
            SetFrameLength(frame, frameLen*GetFrameTime());
            xOffset += width;

            width = 55;
            GUI.Label(new Rect(xOffset, 1, width, rect.height), "Event:", EditorStyles.boldLabel);
            xOffset += width;

            // Function Name
            //width = 125;
            //frame.DefaultEventTrigger = GUI.Toggle(new Rect(xOffset, 0, width, rect.height), frame.DefaultEventTrigger,
            //    "Default Event Trigger", Styles.TIMELINE_EVENT_TOGGLE);
            //xOffset += width;

            width = 80;
            frame.Event =
                (AnimationFrame.EventType)
                    EditorGUI.EnumPopup(new Rect(xOffset, 2, width, rect.height), frame.Event);
            xOffset += width;

            if (frame.Event == AnimationFrame.EventType.None) {
                if (EditorGUI.EndChangeCheck()) {
                    ApplyChanges();
                }
                return;
            }

            width = 50;
            // Frame length (in samples)
            EditorGUI.LabelField(new Rect(xOffset, 2, width, rect.height - 3), "Position");
            xOffset += width + 5;

            width = 150;
            frame.EventPosition.x = EditorGUI.Slider(new Rect(xOffset, 2, width, rect.height - 3), frame.EventPosition.x, -1, 1);
            xOffset += width + 5;
            frame.EventPosition.y = EditorGUI.Slider(new Rect(xOffset, 2, width, rect.height - 3), frame.EventPosition.y, -1, 1);
            xOffset += width + 5;

            width = 70;
            GUI.Label(new Rect(xOffset, 0, width, rect.height), "Event Name:", EditorStyles.miniLabel);
            xOffset += width;

            width = 150;
            // Give gui control name using it's time and function name so it can be auto-selected when creating new evnet
            GUI.SetNextControlName("EventFunctionName");
            frame.EventName = EditorGUI.TextField(new Rect(xOffset, 2, width, rect.height + 5),
                frame.EventName, EditorStyles.miniTextField);
            xOffset += width + 5;

            width = 60;
            frame.EventDataString = EditorGUI.TextField(
                new Rect(xOffset, 2, width, rect.height),
                frame.EventDataString, EditorStyles.miniTextField);
            xOffset += width + 5;

            frame.EventDataFloat = EditorGUI.FloatField(
                new Rect(xOffset, 2, width, rect.height - 3),
                frame.EventDataFloat, EditorStyles.miniTextField);
            xOffset += width + 5;

            width = 150;
            frame.EventDataObject = EditorGUI.ObjectField(
                new Rect(xOffset, 2, width, rect.height - 3), frame.EventDataObject, typeof(Object),
                false);

            if (EditorGUI.EndChangeCheck()) {
                ApplyChanges();
            }
        }

        // NB: if draw is true, it'll draw stuff only, otherwise it'll handle events only.
        private void LayoutEvent(Rect rect, AnimFrame frame, AnimEventLayoutData layoutData, bool draw) {
            // check if it's visible on timeline
            if (layoutData.start > rect.xMax || layoutData.end < rect.xMin)
                return;

            var heightOffset = layoutData.heightOffset*TIMELINE_EVENT_HEIGHT;
            var eventRect = new Rect(layoutData.start, heightOffset, 0, TIMELINE_EVENT_HEIGHT);
            var labelRect = new Rect(layoutData.start + 2, heightOffset, layoutData.textWidth, TIMELINE_EVENT_HEIGHT - 2);

            if (draw)
                LayoutEventVisuals(frame, layoutData.selected, eventRect, layoutData.text, labelRect);
            else
                LayoutEventGuiEvents(rect, frame, layoutData.selected, eventRect, layoutData.text, labelRect);
        }

        // Handles gui events for an event
        private void LayoutEventGuiEvents(Rect rect, AnimFrame frame, bool selected, Rect eventRect,
            string labelText, Rect labelRect) {
            var eventClickRect = new Rect(eventRect) {
                xMin = eventRect.xMin + EVENT_CLICK_OFFSET,
                width = EVENT_CLICK_WIDTH
            };

            //
            // Frame clicking events
            //

            //var e = Event.current;
            //var mouseContained = eventClickRect.Contains(e.mousePosition) || labelRect.Contains(e.mousePosition);
                // Can click on either label or event bar

            // Move cursor (when selected, it can be dragged to move it)	
            EditorGUIUtility.AddCursorRect(eventClickRect, MouseCursor.MoveArrow);

            //if (_dragState == eDragState.None && mouseContained && e.button == 0) {
            //    //
            //    // Handle Event Selection
            //    //
            //    if ((selected == false || e.control) && e.type == EventType.MouseDown) {
            //        // Started clicking unselected - start selecting
            //        SelectEvent(frame);
            //        GUI.FocusControl("none");
            //        e.Use();
            //    }

            //    if (selected && e.control == false && _selectedEvents.Count > 1 && e.type == EventType.MouseUp) {
            //        // Had multiple selected, and clicked on just one, deselect others. Done on mouse up so we can start the drag if we want
            //        SelectEvent(frame);
            //        GUI.FocusControl("none");
            //        e.Use();
            //    }

            //    if (selected && e.type == EventType.MouseDown) {
            //        GUI.FocusControl("none");
            //        // Clicked alredy selected item, consume event so it doesn't get deseleccted when starting a move
            //        e.Use();
            //    }

            //    //
            //    // Handle start move frame drag
            //    //
            //    if (e.type == EventType.MouseDrag) {
            //        _dragState = eDragState.MoveEvent;
            //    }
            //}
        }

        private void LayoutEvents(Rect rect) {
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) && _playing == false) {
                // Move timeline
                _animTime = SnapTimeToFrameRate(GuiPosToAnimTime(rect, e.mousePosition.x));
            }
            //if (e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2 && rect.Contains(e.mousePosition)) {
            //    // Double click for new event at that time

            //    // New event
            //    InsertEvent(GuiPosToAnimTime(rect, e.mousePosition.x), true);
            //    e.Use();
            //}

            GUI.BeginGroup(rect);

            //if (_events.Count == 0) {
            //    GUI.Label(new Rect(0, 0, rect.width, rect.height), "Double click to insert event",
            //        EditorStyles.centeredGreyMiniLabel);
            //}

            // Layout events. This is done in 4 stages so that selected items are drawn on top of (after) non-selected ones, but have their gui events handled first.

            // Calc some metadata about each event (start/end position on timeline, etc). This is stored in a temporary array, parallel to events
            int eventCount = 0;
            for (int i = 0; i < _frames.Count; i++) {
                if (_frames[i].Event != AnimationFrame.EventType.None) {
                    eventCount++;
                }
            }
            var eventTimelineData = new List<AnimEventLayoutData>(eventCount);

            // First loop over and calculate start/end positions of events
            for (var i = 0; i < _frames.Count; ++i) {
                var frame = _frames[i];
                if (_frames[i].Event == AnimationFrame.EventType.None) {
                    continue;
                }
                var eventData = new AnimEventLayoutData();
                eventTimelineData.Add(eventData);
                eventData.frame = frame;
                eventData.start = AnimTimeToGuiPos(rect, SnapTimeToFrameRate(frame.Time));
                eventData.text = frame.EventName; //.Replace(ANIM_EVENT_PREFIX,null);
                eventData.textWidth = Styles.TIMELINE_EVENT_TEXT.CalcSize(new GUIContent(eventData.text)).x;
                eventData.end = eventData.start + eventData.textWidth + 4;
                eventData.selected = _selectedFrames.Contains(_frames[i]);
            }

            var maxEventOffset = 0;

            // Now loop over events and calculate the vertical offset of events so that they don't overlap
            for (var i = 0; i < eventTimelineData.Count; ++i) {
                // Store the offset of everything we're overlapping with in a mask, so we can get the first available offset.
                var usedOffsetsMask = 0;
                var data = eventTimelineData[i];
                for (var j = i - 1; j >= 0; --j) {
                    // check for overlap of items before this one. A
                    var other = eventTimelineData[j];
                    if ((data.start > other.end || data.end < other.start) == false) {
                        // overlaps!
                        usedOffsetsMask |= 1 << other.heightOffset;
                    }
                }

                // Loop through mask to find first available offset.
                while (data.heightOffset < 32 && (usedOffsetsMask & (1 << data.heightOffset)) != 0) {
                    data.heightOffset++;
                }

                if (data.heightOffset > maxEventOffset)
                    maxEventOffset = data.heightOffset;
            }

            // Draw vertical lines where there's an event
            for (var i = 0; i < eventTimelineData.Count; ++i) {
                DrawRect(new Rect(eventTimelineData[i].start, 0, 1, rect.height), Color.grey);
            }

            // First draw events
            for (var i = 0; i < eventTimelineData.Count; ++i) {
                LayoutEvent(rect, eventTimelineData[i].frame, eventTimelineData[i], true);
            }

            // Then handle gui events in reverse order
            for (var i = eventTimelineData.Count - 1; i >= 0; --i) {
                LayoutEvent(rect, eventTimelineData[i].frame, eventTimelineData[i], false);
            }

            GUI.EndGroup();

            // Draw selection rect
            //if (_dragState == eDragState.SelectEvent && Mathf.Abs(_selectionMouseStart - e.mousePosition.x) > 1.0f) {
            //    // Draw selection rect
            //    var selectionRect = new Rect(rect) {
            //        xMin = MathEx.Min(_selectionMouseStart, e.mousePosition.x),
            //        xMax = MathEx.Max(_selectionMouseStart, e.mousePosition.x)
            //    };
            //    DrawRect(selectionRect, COLOR_UNITY_BLUE.WithAlpha(0.1f), COLOR_UNITY_BLUE.WithAlpha(0.6f));
            //}

            // Check for unhandled mouse left click. It should deselect any selected events
            if (e.type == EventType.MouseDown && e.button == 0) {
                if (rect.Contains(e.mousePosition)) {
                    ClearSelection();
                    e.Use();
                }
            }

            // Check for unhanlded drag, it should start a select
            //if (_dragState == eDragState.None && e.type == EventType.MouseDrag && e.button == 0) {
            //    if (rect.Contains(e.mousePosition)) {
            //        _dragState = eDragState.SelectEvent;
            //        e.Use();
            //    }
            //}

            //if (_dragState == eDragState.MoveEvent) {
            //    // While moving frame, show the move cursor
            //    EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            //    if (e.type == EventType.MouseDrag) {
            //        MoveSelectedEvents(e.delta.x);

            //        // move the frame
            //        e.Use();
            //    }
            //    if (e.rawType == EventType.MouseUp && e.button == 0) {
            //        // Apply the move change
            //        ApplyChanges();
            //        _dragState = eDragState.None;
            //        e.Use();
            //    }
            //}

            var newTimelineHeight = MathEx.Max(maxEventOffset + 1, 1.5f)*TIMELINE_EVENT_HEIGHT;
            if (newTimelineHeight != _timelineEventBarHeight) {
                _timelineEventBarHeight = newTimelineHeight;
                Repaint();
            }
        }

        private void LayoutEventsBarBack(Rect rect) {
            GUI.BeginGroup(rect, Styles.TIMELINE_KEYFRAME_BG);
            GUI.EndGroup();
        }

        // draws visuals for an event
        private void LayoutEventVisuals(AnimFrame frame, bool selected, Rect eventRect, string labelText,
            Rect labelRect) {
            // Color differently if selected
            var eventColor = selected ? COLOR_UNITY_BLUE : Color.grey;
            var eventBGColor = selected ? COLOR_EVENT_LABEL_BG_SELECTED : COLOR_EVENT_LABEL_BG;

            //DrawRect( new Rect(eventRect) { width = EVENT_WIDTH }, eventColor  );
            DrawRect(labelRect, eventBGColor);
            GUI.Label(new Rect(labelRect) {
                yMin = labelRect.yMin - 4
            }, labelText, new GUIStyle(Styles.TIMELINE_EVENT_TEXT) {
                normal = {
                    textColor = eventColor
                }
            });
            if (selected) GUI.color = COLOR_UNITY_BLUE;
            GUI.Box(new Rect(eventRect.xMin - 2, eventRect.yMin, 6, 20), Contents.EVENT_MARKER,
                new GUIStyle(Styles.TIMELINE_EVENT_TICK) {
                    normal = {
                        textColor = eventColor
                    }
                });
            GUI.color = Color.white;
        }

        private void LayoutFrame(Rect rect, int frameId, float startTime, float endTime, Sprite sprite) {
            var startOffset = _timelineOffset + startTime*_timelineScale;
            var endOffset = _timelineOffset + endTime*_timelineScale;

            // check if it's visible on timeline
            if (startOffset > rect.xMax || endOffset < rect.xMin)
                return;
            var animFrame = _frames[frameId];
            var frameRect = new Rect(startOffset, 0, endOffset - startOffset, rect.height);
            var selected = _selectedFrames.Contains(animFrame);
            if (selected) {
                // highlight selected frames
                DrawRect(frameRect, Color.grey.WithAlpha(0.3f));
            }
            DrawLine(new Vector2(endOffset, 0), new Vector2(endOffset, rect.height), new Color(0.4f, 0.4f, 0.4f));
            LayoutTimelineSprite(frameRect, GetSpriteAtTime(startTime));

            //
            // Frame clicking events
            //

            var e = Event.current;

            if (_dragState == eDragState.None) {
                // Move cursor (when selected, it can be dragged to move it)
                if (selected) {
                    EditorGUIUtility.AddCursorRect(new Rect(frameRect) {
                        xMin = frameRect.xMin + FRAME_RESIZE_RECT_WIDTH*0.5f,
                        xMax = frameRect.xMax - FRAME_RESIZE_RECT_WIDTH*0.5f
                    }, MouseCursor.MoveArrow);
                }

                //
                // Resize rect
                //
                var resizeRect = new Rect(endOffset - FRAME_RESIZE_RECT_WIDTH*0.5f, 0, FRAME_RESIZE_RECT_WIDTH,
                    rect.height);
                EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);

                //
                // Check for Start Resizing frame
                //
                if (e.type == EventType.MouseDown && e.button == 0 && resizeRect.Contains(e.mousePosition)) {
                    // Start resizing the frame
                    _dragState = eDragState.ResizeFrame;
                    _resizeFrameId = frameId;
                    GUI.FocusControl("none");
                    e.Use();
                }

                //
                // Handle Frame Selection
                //
                if (selected == false && e.type == EventType.MouseDown && e.button == 0 &&
                    frameRect.Contains(e.mousePosition)) {
                    // Started clicking unselected - start selecting
                    _dragState = eDragState.SelectFrame;
                    SelectFrame(animFrame);
                    GUI.FocusControl("none");
                    e.Use();
                }

                if (selected && _selectedFrames.Count > 1 && e.type == EventType.MouseUp && e.button == 0 &&
                    frameRect.Contains(e.mousePosition)) {
                    // Had multiple selected, and clicked on just one, deselect others
                    SelectFrame(animFrame);
                    e.Use();
                }

                //
                // Handle start move frame drag (once selected)
                //
                if (selected && e.type == EventType.MouseDrag && e.button == 0 && frameRect.Contains(e.mousePosition)) {
                    _dragState = eDragState.MoveFrame;
                    e.Use();
                }
                if (selected && e.type == EventType.MouseDown && e.button == 0 && frameRect.Contains(e.mousePosition)) {
                    // Clicked alredy selected item, consume event so it doesn't get deseleccted when starting a move
                    GUI.FocusControl("none");
                    e.Use();
                }
            }
            else if (_dragState == eDragState.ResizeFrame) {
                // Check for resize frame by dragging mouse
                if (e.type == EventType.MouseDrag && e.button == 0 && _resizeFrameId == frameId) {
                    if (selected && _selectedFrames.Count > 1) {
                        // Calc frame end if adding a frame to each selected frame.
                        var currEndTime = animFrame.Time + animFrame.RealLength;
                        var newEndTime = animFrame.Time + animFrame.RealLength;
                        var mouseTime = GuiPosToAnimTime(new Rect(0, 0, position.width, position.height),
                            e.mousePosition.x);
                        var direction = Mathf.Sign(mouseTime - currEndTime);
                        for (var i = 0; i < _selectedFrames.Count; ++i) {
                            if (_selectedFrames[i].Time <= animFrame.Time || i == frameId)
                                newEndTime += GetFrameTime()*direction;
                        }
                        // if mouse time is closer to newEndTime than currEnd time then commit the change
                        if (Mathf.Abs(mouseTime - newEndTime) < Mathf.Abs(mouseTime - currEndTime)) {
                            for (var i = 0; i < _selectedFrames.Count; ++i) {
                                _selectedFrames[i].RealLength = MathEx.Max(GetFrameTime(),
                                    SnapTimeToFrameRate(_selectedFrames[i].RealLength + GetFrameTime()*direction));
                            }
                            RecalcFrameTimes();
                        }
                    }
                    else {
                        var newFrameLength =
                            GuiPosToAnimTime(new Rect(0, 0, position.width, position.height), e.mousePosition.x) -
                            startTime;
                        newFrameLength = MathEx.Max(newFrameLength, 1.0f/_clip.FramesPerSecond);
                        SetFrameLength(frameId, newFrameLength);
                    }

                    e.Use();
                    Repaint();
                }

                // Check for finish resizing frame
                if (e.type == EventType.MouseUp && e.button == 0 && _resizeFrameId == frameId) {
                    _dragState = eDragState.None;
                    ApplyChanges();
                    e.Use();
                }
            }
            else if (_dragState == eDragState.SelectFrame) {
                if (e.type == EventType.MouseUp && e.button == 0) {
                    _dragState = eDragState.None;
                    e.Use();
                }
            }
        }

        private void LayoutFrames(Rect rect) {
            var e = Event.current;

            GUI.BeginGroup(rect, Styles.TIMELINE_ANIM_BG);

            //DrawRect( new Rect(0,0,rect.width,rect.height), new Color(0.3f,0.3f,0.3f,1));

            for (var i = 0; i < _frames.Count; ++i) // NB: ignore final dummy keyframe
            {
                // Calc time of next frame
                LayoutFrame(rect, i, _frames[i].Time, _frames[i].EndTime, _frames[i].Sprite);
            }

            // Draw rect over area that has no frames in it
            if (_timelineOffset > 0) {
                // Before frames start
                DrawRect(new Rect(0, 0, _timelineOffset, rect.height), new Color(0.4f, 0.4f, 0.4f, 0.2f));
                DrawLine(new Vector2(_timelineOffset, 0), new Vector2(_timelineOffset, rect.height),
                    new Color(0.4f, 0.4f, 0.4f));
            }
            var endOffset = _timelineOffset + GetAnimLength()*_timelineScale;
            if (endOffset < rect.xMax) {
                // After frames end
                DrawRect(new Rect(endOffset, 0, rect.width - endOffset, rect.height), new Color(0.4f, 0.4f, 0.4f, 0.2f));
            }

            GUI.EndGroup();

            // Draw selection rect
            if (_dragState == eDragState.SelectFrame && Mathf.Abs(_selectionMouseStart - e.mousePosition.x) > 1.0f) {
                // Draw selection rect
                var selectionRect = new Rect(rect) {
                    xMin = MathEx.Min(_selectionMouseStart, e.mousePosition.x),
                    xMax = MathEx.Max(_selectionMouseStart, e.mousePosition.x)
                };
                DrawRect(selectionRect, COLOR_UNITY_BLUE.WithAlpha(0.1f), COLOR_UNITY_BLUE.WithAlpha(0.6f));
            }

            if (_dragState == eDragState.None) {
                //
                // Check for unhandled mouse left click. It should deselect any selected frames
                //
                if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)) {
                    ClearSelection();
                    e.Use();
                }
                // Check for unhanlded drag, it should start a select
                if (e.type == EventType.MouseDrag && e.button == 0 && rect.Contains(e.mousePosition)) {
                    _dragState = eDragState.SelectFrame;
                    e.Use();
                }
            }
            else if (_dragState == eDragState.ResizeFrame) {
                // While resizing frame, show the resize cursor
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            }
            else if (_dragState == eDragState.MoveFrame) {
                // While moving frame, show the move cursor
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            }
        }

        // Handles drag/drop onto timeline
        private void LayoutInsert(Rect rect) {
            var e = Event.current;

            if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && rect.Contains(e.mousePosition)) {
                if (Array.Exists(DragAndDrop.objectReferences, item => item is Sprite || item is Texture2D)) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                    var closestFrame = MousePosToInsertFrameIndex(rect);
                    LayoutInsertFramesLine(rect, closestFrame);
                    _dragDropHovering = true;

                    if (e.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();
                        var sprites = new List<Sprite>();
                        foreach (var obj in DragAndDrop.objectReferences) {
                            if (obj is Sprite) {
                                sprites.Add(obj as Sprite);
                            }
                            else if (obj is Texture2D) {
                                // Grab all sprites associated with a texture, add to list
                                var path = AssetDatabase.GetAssetPath(obj);
                                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                                foreach (var subAsset in assets) {
                                    if (subAsset is Sprite) {
                                        sprites.Add((Sprite) subAsset);
                                    }
                                }
                            }
                        }

                        // Sort sprites by name and insert
                        using (var comparer = new NaturalComparer()) {
                            sprites.Sort((a, b) => comparer.Compare(a.name, b.name));
                        }
                        InsertFrames(sprites.ToArray(), closestFrame);
                    }
                }
            }

            // The indicator won't update while drag/dropping becuse it's not active, so we hack it using this flag
            if (_dragDropHovering && rect.Contains(e.mousePosition)) {
                var closestFrame = MousePosToInsertFrameIndex(rect);
                LayoutInsertFramesLine(rect, closestFrame);
            }
            else {
                _dragDropHovering = false;
            }

            if (e.type == EventType.DragExited) {
                _dragDropHovering = false;
            }
        }

        // Draws line that shows where frames will be inserted
        private void LayoutInsertFramesLine(Rect rect, int frameId) {
            var time = frameId < _frames.Count ? _frames[frameId].Time : GetAnimLength();
            var posOnTimeline = _timelineOffset + time*_timelineScale;

            // check if it's visible on timeline
            if (posOnTimeline < rect.xMin || posOnTimeline > rect.xMax)
                return;
            DrawLine(new Vector2(posOnTimeline, rect.yMin), new Vector2(posOnTimeline, rect.yMax),
                COLOR_INSERT_FRAMES_LINE);
        }

        // Handles moving frames
        private void LayoutMoveFrame(Rect rect) {
            var e = Event.current;

            if (_dragState == eDragState.MoveFrame) {
                var closestFrame = MousePosToInsertFrameIndex(rect);

                LayoutInsertFramesLine(rect, closestFrame);

                if (e.type == EventType.MouseDrag && e.button == 0) {
                    e.Use();
                }

                if (e.type == EventType.MouseUp && e.button == 0) {
                    // Move selected frame to before closestFrame
                    MoveSelectedFrames(closestFrame);
                    _dragState = eDragState.None;
                    e.Use();
                }
            }
        }

        private void LayoutPlayhead(Rect rect) {
            var offset = rect.xMin + _timelineOffset + _animTime*_timelineScale;
            DrawLine(new Vector2(offset, rect.yMin), new Vector2(offset, rect.yMax), Color.red);
        }

        private void LayoutScrubber(Rect rect) {
            //
            // Calc time scrubber lines
            //
            var minUnitSecond = 1.0f/_clip.FramesPerSecond;
            var curUnitSecond = 1.0f;
            var curCellWidth = _timelineScale;
            int intervalId;
            var intervalScales = CreateIntervalSizeList(out intervalId);

            // get curUnitSecond and curIdx
            if (curCellWidth < SCRUBBER_INTERVAL_WIDTH_MIN) {
                while (curCellWidth < SCRUBBER_INTERVAL_WIDTH_MIN) {
                    curUnitSecond = curUnitSecond*intervalScales[intervalId];
                    curCellWidth = curCellWidth*intervalScales[intervalId];

                    intervalId += 1;
                    if (intervalId >= intervalScales.Count) {
                        intervalId = intervalScales.Count - 1;
                        break;
                    }
                }
            }
            else if (curCellWidth > SCRUBBER_INTERVAL_WIDTH_MAX) {
                while ((curCellWidth > SCRUBBER_INTERVAL_WIDTH_MAX) &&
                       (curUnitSecond > minUnitSecond)) {
                    intervalId -= 1;
                    if (intervalId < 0) {
                        intervalId = 0;
                        break;
                    }

                    curUnitSecond = curUnitSecond/intervalScales[intervalId];
                    curCellWidth = curCellWidth/intervalScales[intervalId];
                }
            }

            // check if prev width is good to show
            if (curUnitSecond > minUnitSecond) {
                var intervalIdPrev = intervalId - 1;
                if (intervalIdPrev < 0)
                    intervalIdPrev = 0;
                var prevCellWidth = curCellWidth/intervalScales[intervalIdPrev];
                var prevUnitSecond = curUnitSecond/intervalScales[intervalIdPrev];
                if (prevCellWidth >= SCRUBBER_INTERVAL_WIDTH_MIN) {
                    intervalId = intervalIdPrev;
                    curUnitSecond = prevUnitSecond;
                    curCellWidth = prevCellWidth;
                }
            }

            // get lod interval list
            var lodIntervalList = new int[intervalScales.Count + 1];
            lodIntervalList[intervalId] = 1;
            for (var i = intervalId - 1; i >= 0; --i) {
                lodIntervalList[i] = lodIntervalList[i + 1]/intervalScales[i];
            }
            for (var i = intervalId + 1; i < intervalScales.Count + 1; ++i) {
                lodIntervalList[i] = lodIntervalList[i - 1]*intervalScales[i - 1];
            }

            // Calc width of intervals
            var lodWidthList = new float[intervalScales.Count + 1];
            lodWidthList[intervalId] = curCellWidth;
            for (var i = intervalId - 1; i >= 0; --i) {
                lodWidthList[i] = lodWidthList[i + 1]/intervalScales[i];
            }
            for (var i = intervalId + 1; i < intervalScales.Count + 1; ++i) {
                lodWidthList[i] = lodWidthList[i - 1]*intervalScales[i - 1];
            }

            // Calc interval id to start from
            var idxFrom = intervalId;
            for (var i = 0; i < intervalScales.Count + 1; ++i) {
                if (lodWidthList[i] > SCRUBBER_INTERVAL_WIDTH_MAX) {
                    idxFrom = i;
                    break;
                }
            }

            // NOTE: +50 here can avoid us clip text so early 
            var iStartFrom = Mathf.CeilToInt(-(_timelineOffset + 50.0f)/curCellWidth);
            var cellCount = Mathf.CeilToInt((rect.width - _timelineOffset)/curCellWidth);

            //DrawRect(rect,new Color(0.25f, 0.25f, 0.25f), new Color(0.1f, 0.1f, 0.1f));

            // draw the scrubber bar
            GUI.BeginGroup(rect, EditorStyles.toolbar);

            for (var i = iStartFrom; i < cellCount; ++i) {
                var x = _timelineOffset + i*curCellWidth + 1;
                var idx = idxFrom;

                while (idx >= 0) {
                    if (i%lodIntervalList[idx] == 0) {
                        var heightRatio = 1.0f - lodWidthList[idx]/SCRUBBER_INTERVAL_WIDTH_MAX;

                        // draw scrubber bar
                        if (heightRatio >= 1.0f) {
                            DrawLine(new Vector2(x, 0),
                                new Vector2(x, TIMELINE_SCRUBBER_HEIGHT),
                                Color.gray);
                            DrawLine(new Vector2(x + 1, 0),
                                new Vector2(x + 1, TIMELINE_SCRUBBER_HEIGHT),
                                Color.gray);
                        }
                        else {
                            DrawLine(new Vector2(x, TIMELINE_SCRUBBER_HEIGHT*heightRatio),
                                new Vector2(x, TIMELINE_SCRUBBER_HEIGHT),
                                Color.gray);
                        }

                        // draw lable
                        if (lodWidthList[idx] >= SCRUBBER_INTERVAL_TO_SHOW_LABEL) {
                            GUI.Label(new Rect(x + 4.0f, -2, 50, 15),
                                ToTimelineLabelString(i*curUnitSecond, _clip.FramesPerSecond), EditorStyles.miniLabel);
                        }

                        //
                        break;
                    }
                    --idx;
                }
            }

            GUI.EndGroup();

            //
            // Scrubber events
            //

            var e = Event.current;
            if (rect.Contains(e.mousePosition)) {
                if (e.type == EventType.MouseDown) {
                    if (e.button == 0) {
                        _dragState = eDragState.Scrub;
                        _animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
                        GUI.FocusControl("none");
                        e.Use();
                    }
                }
            }
            if (_dragState == eDragState.Scrub && e.button == 0) {
                if (e.type == EventType.MouseDrag) {
                    _animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
                    e.Use();
                }
                else if (e.type == EventType.MouseUp) {
                    _dragState = eDragState.None;
                    e.Use();
                }
            }
        }

        private void LayoutTimeline(Rect rect) {
            var e = Event.current;

            // Store mouse x offset when ever button is pressed for selection box
            if (_dragState == eDragState.None && e.rawType == EventType.MouseDown && e.button == 0) {
                _selectionMouseStart = e.mousePosition.x;
            }

            // Select whatever's in the selection box
            if ((_dragState == eDragState.SelectFrame) &&
                e.rawType == EventType.MouseDrag && e.button == 0) {
                var dragTimeStart = GuiPosToAnimTime(rect, _selectionMouseStart);
                var dragTimeEnd = GuiPosToAnimTime(rect, e.mousePosition.x);
                if (dragTimeStart > dragTimeEnd)
                    Utils.Swap(ref dragTimeStart, ref dragTimeEnd);
                //if (_dragState == eDragState.SelectEvent) {
                //    _selectedEvents =
                //        _events.FindAll(frame => frame._time >= dragTimeStart && frame._time <= dragTimeEnd);
                //    _selectedEvents.Sort((a, b) => a._time.CompareTo(b._time));
                //}
                //else {
                    _selectedFrames =
                        _frames.FindAll(
                            frame => frame.Time + frame.RealLength >= dragTimeStart && frame.Time <= dragTimeEnd);
                    _selectedFrames.Sort((a, b) => a.Time.CompareTo(b.Time));
                //}

                GUI.FocusControl("none");
            }

            _timelineScale = Mathf.Clamp(_timelineScale, 10, 10000);

            //
            // Update timeline offset
            //
            _timelineAnimWidth = _timelineScale*GetAnimLength();
            if (_timelineAnimWidth > rect.width/2.0f) {
                _timelineOffset = Mathf.Clamp(_timelineOffset, rect.width - _timelineAnimWidth - rect.width/2.0f,
                    -TIMELINE_OFFSET_MIN);
            }
            else {
                _timelineOffset = -TIMELINE_OFFSET_MIN;
            }

            //
            // Layout stuff
            //
            // Draw scrubber bar
            var elementPosY = rect.yMin;
            var elementHeight = TIMELINE_SCRUBBER_HEIGHT;
            LayoutScrubber(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });
            elementPosY += elementHeight;

            // Draw frames
            elementHeight = rect.height -
                            (elementPosY - rect.yMin + _timelineEventBarHeight + TIMELINE_BOTTOMBAR_HEIGHT);
            var rectFrames = new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            };
            LayoutFrames(rectFrames);
            elementPosY += elementHeight;

            // Draw events bar background
            elementHeight = _timelineEventBarHeight;
            LayoutEventsBarBack(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });

            // Draw playhead (in front of events bar background, but behind events
            LayoutPlayhead(new Rect(rect) {
                height = rect.height - TIMELINE_BOTTOMBAR_HEIGHT
            });

            // Draw events bar
            elementHeight = _timelineEventBarHeight;
            LayoutEvents(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });
            elementPosY += elementHeight;

            // Draw bottom
            elementHeight = TIMELINE_BOTTOMBAR_HEIGHT;
            LayoutBottomBar(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });

            // Draw Frame Reposition
            LayoutMoveFrame(rectFrames);

            // Draw Insert
            LayoutInsert(rectFrames);

            /*
		// Draw selection rect
		if ( e.button == 0 &&  ( _dragState == eDragState.SelectFrame || _dragState == eDragState.SelectEvent) )
		{
			// Draw selection rect
			Rect selectionRect = new Rect(_selectionMouseStart,Vector2.one);
			selectionRect = selectionRect.Encapsulate( new Rect(e.mousePosition,Vector2.one) );
			DrawRect(selectionRect,COLOR_UNITY_BLUE.WithAlpha(0.1f),COLOR_UNITY_BLUE);			
		}
		*/

            //
            // Handle events
            //

            if (rect.Contains(e.mousePosition)) {
                if (e.type == EventType.ScrollWheel) {
                    var scale = 10000.0f;
                    while (_timelineScale/scale < 1.0f || _timelineScale/scale > 10.0f) {
                        scale /= 10.0f;
                    }

                    var oldCursorTime = GuiPosToAnimTime(rect, e.mousePosition.x);

                    _timelineScale -= e.delta.y*scale*0.05f;
                    _timelineScale = Mathf.Clamp(_timelineScale, 10.0f, 10000.0f);

                    // Offset to time at old cursor pos is same as at new position (so can zoom in/out of current cursor pos)
                    _timelineOffset += e.mousePosition.x - AnimTimeToGuiPos(rect, oldCursorTime);

                    Repaint();
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag) {
                    if (e.button == 1 || e.button == 2) {
                        _timelineOffset += e.delta.x;
                        Repaint();
                        e.Use();
                    }
                }
            }

            if (e.rawType == EventType.MouseUp && e.button == 0 &&
                (_dragState == eDragState.SelectFrame)) {
                _dragState = eDragState.None;
                Repaint();
            }
        }

        private void LayoutTimelineSprite(Rect rect, Sprite sprite) {
            if (sprite == null)
                return;

            // Can't display packed sprites at the moment, so don't bother trying
            if (sprite.packed && Application.isPlaying)
                return;

            var scale = 0.85f;
            if (sprite.textureRect.width > 0 && sprite.textureRect.height > 0) {
                var widthScaled = rect.width/sprite.textureRect.width;
                var heightScaled = rect.height/sprite.textureRect.height;
                // Finds best fit for timeline window based on sprite size
                if (widthScaled < heightScaled) {
                    scale *= rect.width/sprite.textureRect.width;
                }
                else {
                    scale *= rect.height/sprite.textureRect.height;
                }
            }

            LayoutFrameSprite(rect, sprite, scale, Vector2.zero, true, false);
        }

        // Returns the point- Set to _frames.Length if should insert after final frame
        private int MousePosToInsertFrameIndex(Rect rect) {
            if (_frames.Count == 0)
                return 0;

            // Find point between two frames closest to mouse cursor so we can show indicator
            var closest = float.MaxValue;
            var animTime = GuiPosToAnimTime(rect, Event.current.mousePosition.x);
            var closestFrame = 0;
            for (; closestFrame < _frames.Count + 1; ++closestFrame) {
                // Loop through frames until find one that's further away than the last from the mouse pos
                // For final iteration it checks the end time of the last frame rather than start time
                var frameStartTime = closestFrame < _frames.Count
                    ? _frames[closestFrame].Time
                    : _frames[closestFrame - 1].EndTime;
                var diff = Mathf.Abs(frameStartTime - animTime);
                if (diff > closest)
                    break;
                closest = diff;
            }

            closestFrame = Mathf.Clamp(closestFrame - 1, 0, _frames.Count);
            return closestFrame;
        }

        public static string ToTimelineLabelString(float seconds, float sampleRate) {
            return string.Format("{0:0}:{1:00}", Mathf.FloorToInt(seconds), seconds%1.0f*100.0f);
        }
    }

}