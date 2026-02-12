export const siteConfig = {
  name: "BodyStack",
  navLinks: [
    { href: "#features", labelKey: "nav.features" },
    { href: "#pricing", labelKey: "nav.pricing" },
    { href: "#faq", labelKey: "nav.faq" },
  ],
} as const;

export type SiteConfig = typeof siteConfig;
