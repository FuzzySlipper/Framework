using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PixelComrades;
using PixelComrades.DungeonCrawler;

// In-game Debug Console / DebugLogManager
// Author: Suleyman Yasir Kula
// 
// Receives debug entries and custom events (e.g. Clear, Collapse, Filter by Type)
// and notifies the recycled list view of changes to the list of debug entries
// 
// - Vocabulary -
// Debug/Log entry: a Debug.Log/LogError/LogWarning/LogException/LogAssertion request made by
//                   the client and intercepted by this manager object
// Debug/Log item: a visual (uGUI) representation of a debug entry
// 
// There can be a lot of debug entries in the system but there will only be a handful of log items 
// to show their properties on screen (these log items are recycled as the list is scrolled)

// An enum to represent filtered log types
public enum LogFilter {
    None = 0,
    Info = 1,
    Warning = 2,
    Error = 4,
    All = 7
}

public class DebugLogManager : MonoBehaviour {
    public static DebugLogManager instance;

    private Transform canvasTR;

    // List of unique debug entries (duplicates of entries are not kept)
    private List<DebugLogEntry> collapsedLogEntries;

    // Filtered list of debug entries to show
    private List<int> indicesOfListEntriesToShow;

    // Number of entries filtered by their types
    private int infoEntryCount, warningEntryCount, errorEntryCount;

    // Filters to apply to the list of debug entries to show
    private bool isCollapseOn;

    private bool isLogWindowVisible = true;

    // Last known position of the log window before it was closed
    private Vector3 lastPosition;
    private LogFilter logFilter = LogFilter.All;
    private Dictionary<LogType, Sprite> logSpriteRepresentations;

    private List<DebugLogItem> pooledLogItems;

    // If the last log item is completely visible (scrollbar is at the bottom),
    // scrollbar will remain at the bottom when new debug entries are received
    private bool snapToBottom = true;

    // The order the collapsedLogEntries are received 
    // (duplicate entries have the same index (value))
    private List<int> uncollapsedLogEntriesIndices;
    private Vector2 windowDragDeltaPosition;

    // Should command input field be cleared after pressing Enter
    public bool clearCommandAfterExecution = true;
    public Text clickedLogItemDetails;
    public ScrollRect clickedLogItemDetailsScrollRect;

    public Image collapseButton;

    public Color collapseButtonNormalColor, collapseButtonSelectedColor;
    public InputField commandInputField;
    public Color filterButtonsNormalColor, filterButtonsSelectedColor;
    public Image filterInfoButton, filterWarningButton, filterErrorButton;
    public Text infoEntryCountText, warningEntryCountText, errorEntryCountText;

    // Visuals for different log types
    public Sprite infoLog, warningLog, errorLog;

    public DebugLogItem logItemPrefab;
    public RectTransform logItemsContainer;

    public ScrollRect logItemsScrollRect;

    // Canvas group to modify visibility of the log window
    public CanvasGroup logWindowCanvasGroup;
    public float logWindowMinHeight = 200f;

    // Minimum size of the console window
    public float logWindowMinWidth = 250f;

    public RectTransform logWindowTR;

    public DebugLogPopup popupManager;

    // Recycled list view to handle the log items efficiently
    public DebugLogRecycledListView recycledListView;

    // Debug console will persist between scenes
    public bool singleton = true;

    void Awake() {
        lastPosition = transform.position;
        DebugLogConsole.AddCommandInstance("exit", "Close", this);
        DebugLogConsole.AddCommandInstance("DebugMouseLock", "DebugMouseLock", this);
        DebugLogConsole.AddCommandInstance("DebugPause", "DebugPause", this);
        DebugLogConsole.AddCommandInstance("DebugMenus", "DebugMenus", this);
        DebugLogConsole.AddCommandInstance("FixMouse", "FixMouse", this);
        DebugLogConsole.AddCommandInstance("FPS", "FPS", this);
        DebugLogConsole.AddCommandInstance("Screenshot", "Screenshot", this);
        DebugLogConsole.AddCommandInstance("FlyCam", "FlyCam", this);
        DebugLogConsole.AddCommandInstance("TestTimers", "TestTimers", this);
        DebugLogConsole.AddCommandInstance("Version", "Version", this);

    }

    private void Version() {
        Log(string.Format("Game Version: {0}", Game.Version), "", LogType.Log);
    }

    private void TestTimers() {
        TimeManager.StartUnscaled(RunTimerTest(1));
    }

    private IEnumerator RunTimerTest(float length) {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        var timer = new UnscaledTimer(length);
        timer.StartTimer();
        var startTime = TimeManager.TimeUnscaled;
        while (timer.IsActive) {
            yield return null;
        }
        watch.Stop();
        Debug.LogFormat("Stop Watch Seconds {0} Ms {1} Manual {2} Timer {3}", watch.Elapsed.TotalSeconds, watch.Elapsed.Milliseconds, TimeManager.TimeUnscaled - startTime, length);
    }

    private void FlyCam() {
        PixelComrades.FlyCam.main.ToggleActive();
    }

    private void Screenshot() {
        ScreenCapture.CaptureScreenshot(
            string.Format( "Screenshots/{0}-{1:MM-dd-yy hh-mm-ss}.png", Game.Title, System.DateTime.Now));
    }

    private void FPS() {
        UIFrameCounter.main.Toggle();
    }

    private void FixMouse() {
        if (GameOptions.MouseLook && !Game.CursorUnlocked) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (GameOptions.MouseLook) {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void DebugMouseLock() {
        Log("MouseUnlocked", Game.CursorUnlockedHolder.Debug(), LogType.Log);
    }

    private void DebugPause() {
        Log("DebugPause", Game.PauseHolder.Debug(), LogType.Log);
    }

    private void DebugMenus() {
        if (UIBasicMenu.OpenMenus.Count == 0) {
            Log("DebugMenus: 0", "", LogType.Log);
        }
        else {
            System.Text.StringBuilder sb = new StringBuilder();
            for (int i = 0; i < UIBasicMenu.OpenMenus.Count; i++) {
                sb.AppendNewLine(UIBasicMenu.OpenMenus[i].gameObject.name);
            }
            Log(string.Format("DebugMenus: {0}", UIBasicMenu.OpenMenus.Count), sb.ToString(), LogType.Log);
        }
    }


    void LateUpdate() {
        if (snapToBottom) {
            logItemsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void OnDisable() {
        // Stop receiving debug entries
        Application.logMessageReceived -= ReceivedLog;

        // Stop receiving commands
        commandInputField.onValidateInput -= OnValidateCommand;
    }


    void OnEnable() {
        // Only one instance of debug console is allowed
        if (instance == null) {
            instance = this;
            pooledLogItems = new List<DebugLogItem>();

            canvasTR = transform;

            // Associate sprites with log types
            logSpriteRepresentations = new Dictionary<LogType, Sprite>();
            logSpriteRepresentations.Add(LogType.Log, infoLog);
            logSpriteRepresentations.Add(LogType.Warning, warningLog);
            logSpriteRepresentations.Add(LogType.Error, errorLog);
            logSpriteRepresentations.Add(LogType.Exception, errorLog);
            logSpriteRepresentations.Add(LogType.Assert, errorLog);

            // Initially, all log types are visible
            filterInfoButton.color = filterButtonsSelectedColor;
            filterWarningButton.color = filterButtonsSelectedColor;
            filterErrorButton.color = filterButtonsSelectedColor;

            // When collapse is disabled and all log types are visible (initial state),
            // the order of the debug entries to show on screen is the same as 
            // the order they were intercepted
            collapsedLogEntries = new List<DebugLogEntry>();
            uncollapsedLogEntriesIndices = new List<int>();
            indicesOfListEntriesToShow = uncollapsedLogEntriesIndices;

            recycledListView.SetLogItemHeight(logItemPrefab.transformComponent.sizeDelta.y);
            recycledListView.SetCollapsedEntriesList(collapsedLogEntries);
            recycledListView.SetEntryIndicesList(indicesOfListEntriesToShow);

            // If it is a singleton object, don't destroy it between scene changes
            //if (singleton)
            //    DontDestroyOnLoad(gameObject);
        }
        else if (this != instance) {
            Destroy(gameObject);
            return;
        }
        Close();
        // Intercept debug entries
        Application.logMessageReceived -= ReceivedLog;
        Application.logMessageReceived += ReceivedLog;

        // Listen for entered commands
        commandInputField.onValidateInput -= OnValidateCommand;
        commandInputField.onValidateInput += OnValidateCommand;

        /*Debug.LogAssertion( "assert" );
        Debug.LogError( "error" );
        Debug.LogException( new System.IO.EndOfStreamException() );
        Debug.LogWarning( "warning" );
        Debug.Log( "log" );*/
    }

    // Determine the filtered list of debug entries to show on screen
    private void FilterLogs() {
        if (logFilter == LogFilter.None) {
            // Show no entry
            indicesOfListEntriesToShow = new List<int>();
        }
        else if (logFilter == LogFilter.All) {
            if (isCollapseOn) {
                // All the unique debug entries will be listed just once.
                // So, list of debug entries to show is the same as the
                // order these unique debug entries are added to collapsedLogEntries
                indicesOfListEntriesToShow = new List<int>(collapsedLogEntries.Count);
                for (var i = 0; i < collapsedLogEntries.Count; i++) {
                    indicesOfListEntriesToShow.Add(i);
                }
            }
            else {
                // Special (and most common) case: when all log types are enabled 
                // and collapse mode is disabled, list of debug entries to show is 
                // the same as the order all the debug entries are received.
                // So, don't create a new list of indices
                indicesOfListEntriesToShow = uncollapsedLogEntriesIndices;
            }
        }
        else {
            // Show only the debug entries that match the current filter
            var isInfoEnabled = (logFilter & LogFilter.Info) == LogFilter.Info;
            var isWarningEnabled = (logFilter & LogFilter.Warning) == LogFilter.Warning;
            var isErrorEnabled = (logFilter & LogFilter.Error) == LogFilter.Error;

            if (isCollapseOn) {
                indicesOfListEntriesToShow = new List<int>(collapsedLogEntries.Count);
                for (var i = 0; i < collapsedLogEntries.Count; i++) {
                    var logEntry = collapsedLogEntries[i];
                    if (logEntry.logTypeSpriteRepresentation == infoLog && isInfoEnabled) {
                        indicesOfListEntriesToShow.Add(i);
                    }
                    else if (logEntry.logTypeSpriteRepresentation == warningLog && isWarningEnabled) {
                        indicesOfListEntriesToShow.Add(i);
                    }
                    else if (logEntry.logTypeSpriteRepresentation == errorLog && isErrorEnabled) {
                        indicesOfListEntriesToShow.Add(i);
                    }
                }
            }
            else {
                indicesOfListEntriesToShow = new List<int>(uncollapsedLogEntriesIndices.Count);
                for (var i = 0; i < uncollapsedLogEntriesIndices.Count; i++) {
                    var logEntry = collapsedLogEntries[uncollapsedLogEntriesIndices[i]];
                    if (logEntry.logTypeSpriteRepresentation == infoLog && isInfoEnabled) {
                        indicesOfListEntriesToShow.Add(uncollapsedLogEntriesIndices[i]);
                    }
                    else if (logEntry.logTypeSpriteRepresentation == warningLog && isWarningEnabled) {
                        indicesOfListEntriesToShow.Add(uncollapsedLogEntriesIndices[i]);
                    }
                    else if (logEntry.logTypeSpriteRepresentation == errorLog && isErrorEnabled) {
                        indicesOfListEntriesToShow.Add(uncollapsedLogEntriesIndices[i]);
                    }
                }
            }
        }

        // Clear the Selected Log Item Details text
        clickedLogItemDetails.text = "";

        // Update the recycled list view
        recycledListView.SetEntryIndicesList(indicesOfListEntriesToShow);
    }

    public static void Log(string logString, string stackTrace, LogType logType) {
        instance.ProcessManualLogEntry(logString, stackTrace, logType);
    }

    private void ProcessManualLogEntry(string logString, string stackTrace, LogType logType) {
        var logEntry = new DebugLogEntry(logString, stackTrace, null);
        logEntry.logTypeSpriteRepresentation = logSpriteRepresentations[logType];

        collapsedLogEntries.Add(logEntry);
        var logEntryIndex = collapsedLogEntries.Count - 1;
        uncollapsedLogEntriesIndices.Add(logEntryIndex);

        if (ShouldAddEntryToFilteredEntries(logEntry.logTypeSpriteRepresentation, false)) {
            indicesOfListEntriesToShow.Add(logEntryIndex);
        }

        UpdateLogDisplays(logType);
    }

    private void ReceivedLog(string logString, string stackTrace, LogType logType) {
        var logEntry = new DebugLogEntry(logString, stackTrace, null);

        // Check if this entry is a duplicate (i.e. has been received before)
        var logEntryIndex = collapsedLogEntries.IndexOf(logEntry);
        var isEntryInCollapsedEntryList = logEntryIndex != -1;
        if (!isEntryInCollapsedEntryList) {
            // It is not a duplicate,
            // add it to the list of unique debug entries
            logEntry.logTypeSpriteRepresentation = logSpriteRepresentations[logType];

            collapsedLogEntries.Add(logEntry);
            logEntryIndex = collapsedLogEntries.Count - 1;
        }
        else {
            // It is a duplicate,
            // increment the original debug item's collapsed count
            logEntry = collapsedLogEntries[logEntryIndex];
            logEntry.count++;
        }

        // Add the index of the unique debug entry to the list
        // that stores the order the debug entries are received
        uncollapsedLogEntriesIndices.Add(logEntryIndex);

        // If this debug entry matches the current filters,
        // add it to the list of debug entries to show
        if (ShouldAddEntryToFilteredEntries(logEntry.logTypeSpriteRepresentation, isEntryInCollapsedEntryList)) {
            indicesOfListEntriesToShow.Add(logEntryIndex);
        }

        UpdateLogDisplays(logType);
    }

    private void UpdateLogDisplays(LogType logType) {
        if (logType == LogType.Log) {
            infoEntryCount++;
            infoEntryCountText.text = "" + infoEntryCount;

            // If debug popup is visible, notify it of the new debug entry
            if (!isLogWindowVisible && popupManager != null) {
                popupManager.NewInfoLogArrived();
            }
        }
        else if (logType == LogType.Warning) {
            warningEntryCount++;
            warningEntryCountText.text = "" + warningEntryCount;

            // If debug popup is visible, notify it of the new debug entry
            if (!isLogWindowVisible && popupManager != null) {
                popupManager.NewWarningLogArrived();
            }
        }
        else {
            errorEntryCount++;
            errorEntryCountText.text = "" + errorEntryCount;

            // If debug popup is visible, notify it of the new debug entry
            if (!isLogWindowVisible && popupManager != null) {
                popupManager.NewErrorLogArrived();
            }
        }

        // If log window is visible, update the recycled list view
        if (isLogWindowVisible) {
            recycledListView.OnLogEntriesUpdated();
        }
    }

    // Does this new entry match the current filter
    private bool ShouldAddEntryToFilteredEntries(Sprite logTypeSpriteRepresentation, bool isEntryInCollapsedList) {
        if (logFilter == LogFilter.None) {
            return false;
        }

        // Special case: if all log types are enabled and collapse mode is disabled, 
        // then don't add the entry to the list of entries to show because 
        // in this case indicesOfListEntriesToShow = uncollapsedLogEntriesIndices and
        // an incoming debug entry is added to uncollapsedLogEntriesIndices, no matter what.
        // So, if we were to add the debug entry to the indicesOfListEntriesToShow explicitly,
        // it would be a duplicate
        if (logFilter == LogFilter.All) {
            if (isCollapseOn && !isEntryInCollapsedList) {
                return true;
            }

            return false;
        }

        if (logTypeSpriteRepresentation == infoLog && (logFilter & LogFilter.Info) == LogFilter.Info ||
            logTypeSpriteRepresentation == warningLog && (logFilter & LogFilter.Warning) == LogFilter.Warning ||
            logTypeSpriteRepresentation == errorLog && (logFilter & LogFilter.Error) == LogFilter.Error) {
            if (isCollapseOn && isEntryInCollapsedList) {
                return false;
            }

            return true;
        }

        return false;
    }

    // Clear button is clicked
    public void ClearButtonPressed() {
        snapToBottom = true;

        infoEntryCount = 0;
        warningEntryCount = 0;
        errorEntryCount = 0;

        infoEntryCountText.text = "0";
        warningEntryCountText.text = "0";
        errorEntryCountText.text = "0";

        collapsedLogEntries.Clear();
        uncollapsedLogEntriesIndices.Clear();
        indicesOfListEntriesToShow.Clear();

        recycledListView.OnLogEntriesUpdated();

        // Clear the Selected Log Item Details text
        clickedLogItemDetails.text = "";
    }

    // Collapse button is clicked
    public void CollapseButtonPressed() {
        // Swap the value of collapse mode
        isCollapseOn = !isCollapseOn;

        snapToBottom = true;

        if (isCollapseOn) {
            collapseButton.color = collapseButtonSelectedColor;
        }
        else {
            collapseButton.color = collapseButtonNormalColor;
        }

        recycledListView.SetCollapseMode(isCollapseOn);

        // Determine the new list of debug entries to show
        FilterLogs();
    }

    // Filtering mode of error logs has been changed
    public void FilterErrorButtonPressed() {
        logFilter = logFilter ^ LogFilter.Error;

        if ((logFilter & LogFilter.Error) == LogFilter.Error) {
            filterErrorButton.color = filterButtonsSelectedColor;
        }
        else {
            filterErrorButton.color = filterButtonsNormalColor;
        }

        FilterLogs();
    }

    // Filtering mode of info logs has been changed
    public void FilterLogButtonPressed() {
        logFilter = logFilter ^ LogFilter.Info;

        if ((logFilter & LogFilter.Info) == LogFilter.Info) {
            filterInfoButton.color = filterButtonsSelectedColor;
        }
        else {
            filterInfoButton.color = filterButtonsNormalColor;
        }

        FilterLogs();
    }

    // Filtering mode of warning logs has been changed
    public void FilterWarningButtonPressed() {
        logFilter = logFilter ^ LogFilter.Warning;

        if ((logFilter & LogFilter.Warning) == LogFilter.Warning) {
            filterWarningButton.color = filterButtonsSelectedColor;
        }
        else {
            filterWarningButton.color = filterButtonsNormalColor;
        }

        FilterLogs();
    }

    // A log item is clicked
    public static void OnLogClicked(int entryIndex) {
        // Show stack trace of the debug entry associated with the clicked log item
        instance.clickedLogItemDetails.text =
            instance.collapsedLogEntries[instance.indicesOfListEntriesToShow[entryIndex]].ToString();

        // Notify recycled list view
        instance.recycledListView.OnLogItemClicked(entryIndex);

        // Move scrollbar of Log Item Details scroll rect to the top
        instance.clickedLogItemDetailsScrollRect.verticalNormalizedPosition = 1f;
    }

    // Value of snapToBottom is changed (user scrolled the list manually)
    public void OnSnapToBottomChanged(bool snapToBottom) {
        this.snapToBottom = snapToBottom;
    }

    // Command field input is changed, check if command is submitted
    public char OnValidateCommand(string text, int charIndex, char addedChar) {
        // If command is submitted
        if (addedChar == '\n') {
            // Clear the command field
            if (clearCommandAfterExecution) {
                commandInputField.text = "";
            }

            if (text.Length > 0) {
                // Execute the command
                DebugLogConsole.ExecuteCommand(text);

                // Snap to bottom and select the latest entry
                OnSnapToBottomChanged(true);

                if (indicesOfListEntriesToShow.Count > 0) {
                    OnLogClicked(indicesOfListEntriesToShow.Count - 1);
                }
            }

            return '\0';
        }

        return addedChar;
    }

    // Debug window is being dragged,
    // set the new position of the window
    public void OnWindowDrag(BaseEventData dat) {
        var eventData = (PointerEventData) dat;

        logWindowTR.position = eventData.position + windowDragDeltaPosition;
    }

    public void OnWindowDragEnded(BaseEventData dat) {
        // If log window is not dropped onto the popup, hide the popup
        if (isLogWindowVisible && popupManager != null) {
            popupManager.OnSetInvisible(false);
        }
    }

    // Debug window is about to be moved on screen,
    // cache the offset between pointer and the window position
    public void OnWindowDragStarted(BaseEventData dat) {
        var eventData = (PointerEventData) dat;

        windowDragDeltaPosition = (Vector2) logWindowTR.position - eventData.position;
        lastPosition = logWindowTR.position;

        // Show the popup that the log window can be dropped onto
        if (popupManager != null) {
            popupManager.OnSetVisible();
        }
    }

    // Debug window is being resized,
    // Set the sizeDelta property of the window accordingly while
    // preventing window dimensions from going below the minimum dimensions
    public void OnWindowResize(BaseEventData dat) {
        var eventData = (PointerEventData) dat;

        var newSize = (eventData.position - (Vector2) logWindowTR.position) / canvasTR.localScale.x;
        newSize.y = -newSize.y;
        if (newSize.x < logWindowMinWidth) {
            newSize.x = logWindowMinWidth;
        }

        if (newSize.y < logWindowMinHeight) {
            newSize.y = logWindowMinHeight;
        }

        logWindowTR.sizeDelta = newSize;

        // Update the recycled list view
        recycledListView.OnViewportDimensionsChanged();
    }

    // Pool an unused log item
    public void PoolLogItem(DebugLogItem logItem) {
        logItem.gameObject.SetActive(false);
        pooledLogItems.Add(logItem);
    }
    
    public void Toggle() {
        if (isLogWindowVisible) {
            Close();
        }
        else {
            Open();
        }
    }

    // Hide the log window
    public void Close() {
        PixelComrades.Game.RemoveCursorUnlock("DebugManager");
        logWindowCanvasGroup.interactable = false;
        logWindowCanvasGroup.blocksRaycasts = false;
        logWindowCanvasGroup.alpha = 0f;

        isLogWindowVisible = false;
        PlayerInput.AllInputBlocked = false;
    }

    // Show the log window
    public void Open() {
        PixelComrades.Game.CursorUnlock("DebugManager");
        // Set the position of the window to its last known position
        logWindowTR.position = lastPosition;

        // Update the recycled list view (in case new entries were
        // intercepted while log window was hidden)
        recycledListView.OnLogEntriesUpdated();

        logWindowCanvasGroup.interactable = true;
        logWindowCanvasGroup.blocksRaycasts = true;
        logWindowCanvasGroup.alpha = 1f;

        isLogWindowVisible = true;
        PlayerInput.AllInputBlocked = true;
        EventSystem.current.SetSelectedGameObject(commandInputField.gameObject);
    }

    // Fetch a log item from the pool
    public DebugLogItem UnpoolLogItem() {
        DebugLogItem newLogItem;

        // If pool is not empty, fetch a log item from the pool,
        // create a new log item otherwise
        if (pooledLogItems.Count > 0) {
            newLogItem = pooledLogItems[pooledLogItems.Count - 1];
            pooledLogItems.RemoveAt(pooledLogItems.Count - 1);
            newLogItem.gameObject.SetActive(true);
        }
        else {
            newLogItem = Instantiate(logItemPrefab);
            newLogItem.transformComponent.SetParent(logItemsContainer, false);
        }

        return newLogItem;
    }
}