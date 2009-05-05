ASSEMBLY = $(top_builddir)/bin/Cubano.exe

SOURCES =  \
	Banshee.Gui.Widgets/ConnectedSeekableTrackInfoDisplay.cs \
	Banshee.Gui.Widgets/CoverArtBrush.cs \
	Banshee.Gui.Widgets/CoverArtDisplay.cs \
	Banshee.Gui.Widgets/SeekableTrackInfoDisplay.cs \
	Cubano.Client/CubanoClient.cs \
	Cubano.Client/CubanoMainMenu.cs \
	Cubano.Client/CubanoSourceContents.cs \
	Cubano.Client/CubanoTheme.cs \
	Cubano.Client/CubanoTitleCell.cs \
	Cubano.Client/CubanoTrackTitleView.cs \
	Cubano.Client/CubanoVisualizer.cs \
	Cubano.Client/CubanoWindow.cs \
	Cubano.Client/CubanoWindowDecorator.cs \
	Cubano.Client/LtrTrackSourceContents.cs \
	Cubano.Client/SearchEntry.cs \
	Cubano.Client/ViewContainer.cs \
	Cubano.Client/WindowDecorator.cs \
	Cubano.NowPlaying/NowPlayingInterface.cs \
	Cubano.NowPlaying/NowPlayingSource.cs \
	Hyena.Gui.Canvas/AnimationManager.cs \
	Hyena.Gui.Canvas/Brush.cs \
	Hyena.Gui.Canvas/CanvasHost.cs \
	Hyena.Gui.Canvas/CanvasItem.cs \
	Hyena.Gui.Canvas/CanvasItemCollection.cs \
	Hyena.Gui.Canvas/CanvasManager.cs \
	Hyena.Gui.Canvas/FixedPanel.cs \
	Hyena.Gui.Canvas/FontWeight.cs \
	Hyena.Gui.Canvas/Image.cs \
	Hyena.Gui.Canvas/ImageBrush.cs \
	Hyena.Gui.Canvas/MarginStyle.cs \
	Hyena.Gui.Canvas/Orientation.cs \
	Hyena.Gui.Canvas/Panel.cs \
	Hyena.Gui.Canvas/Rect.cs \
	Hyena.Gui.Canvas/ShadowMarginStyle.cs \
	Hyena.Gui.Canvas/Size.cs \
	Hyena.Gui.Canvas/Slider.cs \
	Hyena.Gui.Canvas/StackPanel.cs \
	Hyena.Gui.Canvas/TestTile.cs \
	Hyena.Gui.Canvas/TextBlock.cs \
	Hyena.Gui.Canvas/TextWrap.cs \
	Hyena.Gui.Canvas/Thickness.cs

RESOURCES =

REFERENCES_PACKAGES =  \
	-pkg:banshee-1-core \
	-pkg:banshee-1-hyena \
	-pkg:banshee-1-hyena-gui \
	-pkg:banshee-1-services \
	-pkg:banshee-1-thickclient \
	-pkg:clutter-gtk-sharp \
	-pkg:clutter-sharp \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	-r:Mono.Cairo

RESOURCES_BUILD = $(foreach res, $(RESOURCES), \
	-resource:$(notdir $(res)),$(res))

REFERENCES_ASSEMBLIES = ../../bin/Mono.Addins.dll
REFERENCES_ASSEMBLIES_BUILD = $(foreach ref, $(REFERENCES_ASSEMBLIES), \
	-r:$(srcdir)/$(ref))

REFERENCES_BUILD = $(BANSHEE_LIBS) $(CLUTTER_LIBS) $(REFERENCES_ASSEMBLIES_BUILD)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES) $(RESOURCES)
	$(MCS) -out:$@ -debug $(REFERENCES_BUILD) $(RESOURCES_BUILD) $(addprefix $(srcdir)/, $(SOURCES))

run:
	@(cd $(top_builddir)/bin; make run)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY){,.mdb}
MAINTAINERCLEANFILES = Makefile.in
