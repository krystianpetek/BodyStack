import React from 'react'

type LandingContainerProps = {
  children: React.ReactNode
  className?: string
}

export default function LandingContainer({ children, className }: LandingContainerProps) {
  return <div className={`mx-auto w-full max-w-6xl px-4 sm:px-6 ${className ?? ''}`}>{children}</div>
}
