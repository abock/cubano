SUBDIRS = \
	src \
	bin \
	po

all:
	echo $(CLUTTER_BUNDLE_FILES)

install-data-local:
	for lib in $(CLUTTER_BUNDLE_FILES); do \
		$(INSTALL_DATA) $$lib $(DESTDIR)$(libdir)/banshee-1; \
	done

uninstall-hook:
	for lib in $(CLUTTER_BUNDLE_FILES); do \
		rm -f $(DESTDIR)$(libdir)/banshee-1/$$(basename $$lib); \
	done

run:
	@(cd $(top_builddir)/bin; make -s run)

EXTRA_DIST = \
	intltool-extract.in \
	intltool-merge.in \
	intltool-update.in \
	AUTHORS

CLEANFILES = \
	intltool-extract \
	intltool-update \
	intltool-merge

MAINTAINERCLEANFILES = \
	compile \
	INSTALL \
	config.h.in \
	aclocal.m4 \
	ltmain.sh \
	Makefile.in \
	depcomp \
	missing \
	install-sh \
	configure \
	config.sub \
	config.guess \
	intltool-extract.in \
	intltool-merge.in   \
	intltool-update.in  \
	mkinstalldirs
