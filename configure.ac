AC_PREREQ(2.52)

m4_define(cubano_version_major, 1)
m4_define(cubano_version_minor, 0)
m4_define(cubano_version_micro, 0alpha2)

AC_INIT([cubano],
	[cubano_version_major.cubano_version_minor.cubano_version_micro])

AC_CONFIG_SRCDIR(README)
AC_CANONICAL_SYSTEM

AM_INIT_AUTOMAKE([1.9 dist-bzip2 tar-ustar])
AM_MAINTAINER_MODE

PKG_PROG_PKG_CONFIG

GETTEXT_PACKAGE=cubano
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, ["$GETTEXT_PACKAGE"], 
	[Define to the Gettext package name])
AC_SUBST(GETTEXT_PACKAGE)
AM_GLIB_GNU_GETTEXT
IT_PROG_INTLTOOL([0.40.0]) 

PKG_CHECK_EXISTS(MONO, mono >= 2.0)
PKG_CHECK_MODULES(BANSHEE, banshee-1-thickclient >= 1.5.0)
PKG_CHECK_MODULES(CLUTTER, clutter-sharp >= 0.9 clutter-gtk-sharp >= 0.9)
AC_SUBST(BANSHEE_LIBS)
AC_SUBST(CLUTTER_LIBS)
AC_PATH_PROG(MONO, mono)
AC_PATH_PROG(MCS, gmcs)

BANSHEE_PATH=`pkg-config --variable=bansheedir banshee-1-core`
AC_SUBST(BANSHEE_PATH)

#AC_ARG_WITH(moonlight-source, AC_HELP_STRING([--with-moonlight-source=<path>],
#	[Path to the top of the Moonlight source tree]),
#	[], [with_moonlight_source=$PWD/../moon])
#AC_SUBST([MOONLIGHT_SOURCE], ["$with_moonlight_source"])

#if ! grep -q "^PACKAGE_NAME='moonlight'$" $with_moonlight_source/configure; then
#	AC_ERROR([$with_moonlight_source does not appear to be a valid configured Moonlight source directory])
#fi

AC_OUTPUT([
Makefile
po/Makefile.in
bin/Makefile
src/Makefile
src/Cubano/Makefile
])

cat <<EOF

Cubano Configuration
--------------------

  Install Prefix:       $prefix
  Banshee Runtime Path: $BANSHEE_PATH

  Mono Runtime:         $MONO
  C# Compiler:          $MCS

EOF

#   Moonlight Source:     $MOONLIGHT_SOURCE

