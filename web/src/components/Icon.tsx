import { getIconUrl } from '@/assets/icons'

interface IconProps {
  name: string
  size?: number
  className?: string
}

export function Icon({ name, size = 16, className }: IconProps) {
  const url = getIconUrl(name)
  if (!url) return null

  return (
    <img
      src={url}
      width={size}
      height={size}
      className={className}
      alt={name}
      draggable={false}
    />
  )
}
