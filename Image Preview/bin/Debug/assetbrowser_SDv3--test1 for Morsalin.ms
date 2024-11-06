(		
	setIniForceUTF16Default true
	
	global 	createrolloutasset
	global SD_Asset_browser
	global prevfile
	global pickedfile, pickedfileroot
	global rootfolder = "I:\\_image database 2023\\" 
	global pickedfolder	
	global matdir, modeldir	
	
	global dncHHImageList,dncImagePreview
	global	thumbname_tomove	
	global	libraryname_tomove
	global 	temprolloutvalue
	--global localconfig_file
	global excludeChildren
	global mat_2_sel		
	global DnColorClass = dotnetclass "system.drawing.color"
	global tempthumbpath=matdir+"\\tempthumb.jpg"
	global favmodelfile	--		=(matdir+"\\favouritelist.ini")
	
	global modelcontrols=#("getmodelbtn","Replacemodelbtn","xrefmodelbtn","Saveselectionbtn","Save_each_as_zipbtn","replace_library_modelbtn","update_librry_modelbtn")	--	classof SD_Asset_browser.renderalllibrarybtn
	global materialcontrols=#("rendermathumbbtn","renderselthumbbtn","Mat_to_Selbtn","ReplaceActiveMatbtn","Mat_to_Editorbtn","overwritechk","renderalllibrarybtn","Refreshlibrarybtn","copymatsettingbtn",
							"Deletematerialbtn","putMat2librarybtn","refreshbtn","Add_2_multiMaterialbtn","Replace_submatbtn","Copymatbtn","Cutmatbtn","Renamematbtn","putSelmat2librarybtn")

	global	SetupUI,imagelistupdate,recurseHierarchy,update_interface,populateTreeView,disable_controls
	global 	Bgradelib	--="D\\model library B grade"
 	
try( dotnet.loadassembly "C:\\Users\\zahid\\Desktop\\Sayedul\\ImagePreview\\Image Preview\\bin\\Debug\\"+ @"\Image Preview.dll")catch() 	
try(dotnet.loadassembly  (getFilenamePath  (getThisScriptFilename())+@"\Image Preview.dll") )catch() 								

rollout SD_Asset_browser "StudioD'xine asset browser" width:1100 height:960
(
	
	local rootObjs=#()		--Will hold all the root nodes found in the scene. 
	local d=dotnetclass "system.io.directory"	
	local	imagefilearray = #()
	dotNetControl 'dncHHImageList' "Image_Preview.UserControl1" pos:[266,9] width:706 height:436 align:#left
	dotNetControl 'tv' "system.windows.forms.treeView" pos:[13,83] width:235 height:857 align:#left
	
	checkbox 'chkfav' "Show Fav Only" pos:[988,457] width:96 height:29 checked:false align:#left
	button 'btn5' "Browse" pos:[76,47] width:100 height:18 align:#left
	
	fn SetupUI =
	(	
	)
	
fn imagelistupdate foldername=(
	
--	dncHHImageList.populate "C:\\Users\\zahid\Desktop\\minimal\\" #()
-- 	
	imagefilearray=#()
	if 	SD_Asset_browser.chkfav.state==true then (
		list=getINISetting favmodelfile	pickedfolder   -- f=list[1]
		for f in list  do (if not doesFileExist f  then (delINISetting  favmodelfile	pickedfolder f; deleteItem list (findItem list f )  ) )                     
		if list.count!=0 then (SD_Asset_browser.dncHHImageList.populate list imagefilearray	) else (SD_Asset_browser.dncHHImageList.populate foldername imagefilearray	)
		)
	else	SD_Asset_browser.dncHHImageList.populate foldername imagefilearray
		
		)
		
	fn findRootObjs rootfolder=
	(		
		rootObjs=#()
		--	x.parent==undefined then append rootObjs x
		rootObjs= (d.GetDirectories rootfolder)		
	)
	
	
	--Recurse hierarchy and add treeview nodes.
	fn recurseHierarchy obj theTvNode =
	(	--oldnodes=tv.Nodes
		--showmethods oldnodes
		if doesfileexist obj then (
		for i = 1 to (d.GetDirectories  obj).count do		--Loop through each of the children
		(	childdir=(d.GetDirectories obj)[i]
			
			n=(dotNetObject "System.Windows.Forms.TreeNode" (filenamefrompath	childdir))
			n.name=childdir	
			theTvNode.nodes.add n
			recurseHierarchy childdir n 	--Call recursion on each of the children. 			
		)
		)
	)	
	
	--Adds root nodes to treeView.
	fn populateTreeView theTv	rootfolder=
	(	rootObjs=#()
		theTv.Nodes.clear()---clearing old nodes		
		if (	getINISetting localconfig_file "time")[1] !=	((getLocalTime())[4] as string )	then
			(	rootObjs=findRootObjs	rootfolder 
				delINISetting localconfig_file rootfolder
			for x in rootObjs do(setINISetting localconfig_file rootfolder x "on" )
								setINISetting localconfig_file "time" ((getLocalTime())[4] as string) "on" 
			)		--Collect all the root objects.
		else (rootObjs=getINISetting localconfig_file rootfolder	)
		for x in rootObjs do 
		(	--Create a treeViewNode and add it to the treeView control
			n=(dotNetObject "System.Windows.Forms.TreeNode" (filenamefrompath 	x))
			n.name=	x	
		theTv.nodes.add n	
		recurseHierarchy x n 	--Call recursive function on each of the root nodes. 
		)
	)
	--  rootfolder=matdir	
	fn update_interface=(
			)	
			
	
	on SD_Asset_browser open do
	(--	SetupUI()
	--	if pickedfolder==undefined then pickedfolder=(getinisetting localconfig_file "assetbrowser" "lastfolder" )
	--	if  doesfileexist pickedfolder then
			(populateTreeView tv rootfolder	)
			imagelistupdate	pickedfolder	
		--	SD_Asset_browser.dncHHImageList.populate "\\\\flux-server\\Model library\\animal"  #()		
		--	showEvents 	dncHHImageList		
	)
	
	
	on SD_Asset_browser okToClose do
	(setINISetting localconfig_file "assetbrowser" "lastfolder"  (pickedfolder as string) 
	--	SD_Asset_browser.dncHHImageList.housekeeping()
		enableaccelerators = true
		--doesfileexist localconfig_file
		
	)
	on dncHHImageList ThumbPicked args do
	(		pickedfile=SD_Asset_browser.dncHHImageList.currentitem
			pickedfileroot=substituteString pickedfile "-screenshot" ""
			pickedfileroot=substituteString pickedfileroot ".jpg" ""
			pickedfileroot=substituteString pickedfileroot ".max" ""
			pickedfileroot=substituteString pickedfileroot ".zip" ""		
			print pickedfile

	)
	on tv mouseDown arg do
	(mynode=(tv.GetNodeAt arg.location)
	
		pickedfolder=mynode.name
		imagelistupdate pickedfolder
		print pickedfolder
	--	print SD_Asset_browser.dncHHImageList.currentitem
	)
	on chkfav changed state do
		(  imagelistupdate pickedfolder  )
	on btn5 pressed do
	(mypath = thePath = getSavepath()
		(populateTreeView tv mypath	)
		)
)

fn createrolloutasset=
	(
	sdassetbrowser=newRolloutFloater  "asset browser" 1120 980
	addRollout  SD_Asset_browser  sdassetbrowser		
--		)	
--	createdialog SD_Asset_browser 1120 980
)
		createrolloutasset()
)
	