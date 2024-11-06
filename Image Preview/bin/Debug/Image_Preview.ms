try (
    -- Try loading the C# assembly
    dotnet.loadassembly (getFilenamePath (getThisScriptFilename()) + @"\Image Preview.dll")
) catch (
    print "Error loading Image Preview.dll"
)

rollout imageloader "Small Browser" width:610 height:600
(
    -- Load the custom user control from the C# assembly
    dotNetControl small_dialog "Image_Preview.UserControl1" pos:[0, 0] width:600 height:480
    dotNetControl searchBox "System.Windows.Forms.TextBox" width:100 height:30 pos:[20, 490]

    on searchBox KeyUp s e do
    (
        local searchText = searchBox.Text
        print ("Key data: " + searchText)
        small_dialog.SearchLoad(searchText)  -- Assuming 'SearchLoad' exists in the C# control
    )

    button b01 "Refresh" pos:[10, 520]
    on b01 pressed do
    (
        -- Specify the directory path and load images into the dialog
        local directoryPath = @"C:\Users\mdsai\OneDrive\Desktop\"
        local fileArray = getFiles (directoryPath + "*.jpg")  -- Assuming getFiles is implemented
        small_dialog.Populate(fileArray)  -- Assuming 'Populate' exists in the C# control
    )

    button b02 "Select Directory" pos:[100, 520]
    on b02 pressed do
    (
        local mypath = getSavepath()
       
           
            small_dialog.Populate(mypath)
            print mypath
        
    )

    -- Event handler for when a thumbnail is clicked in the custom control
    on small_dialog.ThumbPicked sender args do
    (
        print "Thumbnail Clicked From Event"
        print small_dialog.CurrentItem  -- Assuming 'CurrentItem' exists in the C# control
    )
)

-- Create and show the dialog with the user control
imagedialog = newRolloutFloater "Small Browser" 610 600
addRollout imageloader imagedialog

-- Auto resize the rollout for better display

