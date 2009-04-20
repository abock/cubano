ASSEMBLY = $(top_builddir)/bin/Cubano.exe

ass:

SOURCES =  \
	Banshee.Gui.Widgets/ConnectedSeekableTrackInfoDisplay.cs \
	Banshee.Gui.Widgets/CoverArtBrush.cs \
	Banshee.Gui.Widgets/CoverArtDisplay.cs \
	Banshee.Gui.Widgets/SeekableTrackInfoDisplay.cs \
	Cubano.Client/CubanoClient.cs \
	Cubano.Client/CubanoWindow.cs \
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

REFERENCES_PACKAGES =  \
	-pkg:banshee-1-core \
	-pkg:banshee-1-hyena \
	-pkg:banshee-1-hyena-gui \
	-pkg:banshee-1-services \
	-pkg:banshee-1-thickclient \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	-r:Mono.Cairo

REFERENCES_ASSEMBLIES = -r:../../bin/Mono.Addins.dll

REFERENCES_BUILD = $(REFERENCES_PACKAGES) $(REFERENCES_ASSEMBLIES)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) -out:$@ -debug $(REFERENCES_BUILD) $(SOURCES)

run:
	@(cd $(top_builddir)/bin; make run)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY){,.mdb}
MAINTAINERCLEANFILES = Makefile.in
