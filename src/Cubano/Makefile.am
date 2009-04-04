ASSEMBLY = $(top_builddir)/bin/Cubano.exe

SOURCES = \
	Cubano.Client/Entry.cs

REFERENCES = \
	../../bin/System.Windows.dll \
	../../bin/System.Windows.Controls.dll \
	../../bin/System.Windows.Controls.Data.dll \
	../../bin/Moonlight.Gtk.dll

REFERENCES_EXPANDED = $(addprefix $(srcdir)/, $(REFERENCES))
REFERENCES_BUILD = $(foreach ref, $(REFERENCES_EXPANDED), -r:$(ref))
REFERENCES_BUILD += $(BANSHEE_LIBS)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) -out:$@ -debug $(REFERENCES_BUILD) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY){,.mdb}
MAINTAINERCLEANFILES = Makefile.in
