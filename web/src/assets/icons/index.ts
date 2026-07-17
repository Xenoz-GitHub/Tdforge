const modules = import.meta.glob<{ default: string }>('./*.svg', { eager: true })

export const iconUrls: Record<string, string> = {}
for (const [path, mod] of Object.entries(modules)) {
  const name = path.replace('./', '').replace('.svg', '')
  iconUrls[name] = mod.default
}

export function getIconUrl(name: string): string | undefined {
  return iconUrls[name]
}
