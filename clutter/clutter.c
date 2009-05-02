#include <clutter-gtk.h>
#include <clutter/x11/clutter-x11.h>
#include <gdk/gdkx.h>
#include <cairo.h>

gint main ()
{
    const ClutterColor color = { 0, 255, 0, 128 };

    GtkWidget *window;
    GtkWidget *embed;
    ClutterActor *actor, *actor2;
    ClutterActor *stage;
    cairo_t *cr;

    gtk_init (NULL, NULL);
    clutter_x11_set_display (
        gdk_x11_display_get_xdisplay (
            gdk_display_get_default ()));
    clutter_x11_disable_event_retrieval ();
    clutter_init (NULL, NULL);

    window = gtk_window_new (GTK_WINDOW_TOPLEVEL);
    gtk_window_set_title (GTK_WINDOW (window), "Clutter Test");
    gtk_window_set_position (GTK_WINDOW (window), GTK_WIN_POS_CENTER);

    embed = gtk_clutter_embed_new ();
    stage = gtk_clutter_embed_get_stage (GTK_CLUTTER_EMBED (embed));

    actor = clutter_rectangle_new ();
    clutter_rectangle_set_color (CLUTTER_RECTANGLE (actor), &color);
    clutter_actor_set_size (actor, 100, 200);

    actor2 = clutter_cairo_texture_new (50, 50);
    clutter_actor_set_size (actor2, 50, 50);
    clutter_actor_set_position (actor2, 10, 10);
    cr = clutter_cairo_texture_create (CLUTTER_CAIRO_TEXTURE (actor2));

    cairo_rectangle (cr, 0, 0, 50, 50);
    cairo_set_source_rgba (cr, 1, 0, 0, 0.5);
    cairo_fill (cr);

    cairo_destroy (cr);

    clutter_container_add (CLUTTER_CONTAINER (stage), actor, actor2, NULL);
    
    gtk_container_add (GTK_CONTAINER (window), embed);
    gtk_widget_show_all (window);

    gtk_main ();

    return 0;
}

