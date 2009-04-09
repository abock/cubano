ASSEMBLY = $(top_builddir)/bin/Cubano.exe

SOURCES =  \
	Cubano.Client/CubanoClient.cs \
	Cubano.Client/CubanoWindow.cs \
	Cubano.Client/SeekableTrackInfoDisplay.cs

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
